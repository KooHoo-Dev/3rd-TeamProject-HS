using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

namespace WeGotOne
{
    public class BridgeTable
    {
        private const int BridgeCount = 15;
        private const string Header = "BridgeIndex\tRequiredValue\tMinimumItems";

        private readonly Dictionary<int, BridgeRef> bridges;

        public BridgeTable(TextAsset bridgeTsv, IEnumerable<ItemScriptableObject> items)
        {
            if (bridgeTsv == null)
            {
                throw new ArgumentNullException(nameof(bridgeTsv));
            }

            Dictionary<int, ItemScriptableObject> itemById = CreateItemLookup(items);
            List<BridgeDto> bridgeDtos = ParseBridgeDtos(bridgeTsv.text);
            bridges = CreateBridgeReferences(bridgeDtos, itemById);
        }

        public BridgeRef Get(int bridgeIndex)
        {
            return bridges[bridgeIndex];
        }

        public bool TryGet(int bridgeIndex, out BridgeRef bridgeRef)
        {
            return bridges.TryGetValue(bridgeIndex, out bridgeRef);
        }

        private static Dictionary<int, ItemScriptableObject> CreateItemLookup(
            IEnumerable<ItemScriptableObject> items)
        {
            if (items == null)
            {
                throw new ArgumentNullException(nameof(items));
            }

            var itemById = new Dictionary<int, ItemScriptableObject>();
            foreach (ItemScriptableObject item in items)
            {
                if (item == null)
                {
                    throw new InvalidOperationException("ItemScriptableObject 목록에 null이 포함되어 있습니다.");
                }

                if (itemById.ContainsKey(item.Key))
                {
                    throw new InvalidOperationException($"중복된 Item Key가 있습니다: {item.Key}");
                }

                itemById.Add(item.Key, item);
            }

            return itemById;
        }

        private static List<BridgeDto> ParseBridgeDtos(string tsvText)
        {
            if (string.IsNullOrEmpty(tsvText))
            {
                throw new InvalidOperationException("Bridge TSV가 비어 있습니다.");
            }

            string[] rows = tsvText.Replace("\r\n", "\n").Replace('\r', '\n').Split('\n');
            int rowCount = rows.Length;
            while (rowCount > 0 && rows[rowCount - 1] == string.Empty)
            {
                rowCount--;
            }

            if (rowCount == 0)
            {
                throw new InvalidOperationException("Bridge TSV가 비어 있습니다.");
            }

            string header = rows[0];
            if (header.Length > 0 && header[0] == '\uFEFF')
            {
                header = header.Substring(1);
            }

            if (header != Header)
            {
                throw new InvalidOperationException("Bridge TSV 헤더가 올바르지 않습니다.");
            }

            var bridgeDtos = new List<BridgeDto>();
            for (int i = 1; i < rowCount; i++)
            {
                string[] values = rows[i].Split('\t');
                if (values.Length != 3 ||
                    !int.TryParse(values[0], out int bridgeIndex) ||
                    !int.TryParse(values[1], out int requiredValue))
                {
                    throw new InvalidOperationException($"Bridge TSV {i + 1}번째 행이 올바르지 않습니다.");
                }

                bridgeDtos.Add(new BridgeDto
                {
                    BridgeIndex = bridgeIndex,
                    RequiredValue = requiredValue,
                    MinimumItems = values[2],
                });
            }

            return bridgeDtos;
        }

        private static Dictionary<int, BridgeRef> CreateBridgeReferences(
            List<BridgeDto> bridgeDtos,
            Dictionary<int, ItemScriptableObject> itemById)
        {
            var bridgeByIndex = new Dictionary<int, BridgeRef>();

            foreach (BridgeDto bridgeDto in bridgeDtos)
            {
                if (bridgeDto.BridgeIndex < 1 || bridgeDto.BridgeIndex > BridgeCount)
                {
                    throw new InvalidOperationException($"BridgeIndex 범위를 벗어났습니다: {bridgeDto.BridgeIndex}");
                }

                if (bridgeByIndex.ContainsKey(bridgeDto.BridgeIndex))
                {
                    throw new InvalidOperationException($"중복된 BridgeIndex가 있습니다: {bridgeDto.BridgeIndex}");
                }

                if (bridgeDto.RequiredValue <= 0)
                {
                    throw new InvalidOperationException(
                        $"RequiredValue는 0보다 커야 합니다: {bridgeDto.BridgeIndex}");
                }

                Dictionary<int, int> minimumRequirements = ParseMinimumRequirements(
                    bridgeDto.MinimumItems,
                    bridgeDto.BridgeIndex);

                foreach (KeyValuePair<int, int> requirement in minimumRequirements)
                {
                    if (!itemById.TryGetValue(requirement.Key, out ItemScriptableObject item))
                    {
                        throw new InvalidOperationException(
                            $"존재하지 않는 ItemId가 요구됩니다: {requirement.Key}");
                    }

                    if (item.BuildValue <= 0)
                    {
                        throw new InvalidOperationException(
                            $"BuildValue가 0 이하인 ItemId가 요구됩니다: {requirement.Key}");
                    }
                }

                bridgeByIndex.Add(
                    bridgeDto.BridgeIndex,
                    new BridgeRef(
                        bridgeDto.BridgeIndex,
                        bridgeDto.RequiredValue,
                        minimumRequirements));
            }

            for (int bridgeIndex = 1; bridgeIndex <= BridgeCount; bridgeIndex++)
            {
                if (!bridgeByIndex.ContainsKey(bridgeIndex))
                {
                    throw new InvalidOperationException($"누락된 BridgeIndex가 있습니다: {bridgeIndex}");
                }
            }

            return bridgeByIndex;
        }

        private static Dictionary<int, int> ParseMinimumRequirements(
            string minimumItems,
            int bridgeIndex)
        {
            var minimumRequirements = new Dictionary<int, int>();
            if (minimumItems == string.Empty)
            {
                return minimumRequirements;
            }

            string[] entries = minimumItems.Split(',');
            foreach (string entry in entries)
            {
                string[] values = entry.Split(':');
                if (values.Length != 2 ||
                    !int.TryParse(
                        values[0],
                        NumberStyles.AllowLeadingSign,
                        CultureInfo.InvariantCulture,
                        out int itemId) ||
                    !int.TryParse(
                        values[1],
                        NumberStyles.None,
                        CultureInfo.InvariantCulture,
                        out int minimumCount) ||
                    minimumCount <= 0)
                {
                    throw new InvalidOperationException(
                        $"BridgeIndex {bridgeIndex}의 MinimumItems 형식이 올바르지 않습니다.");
                }

                if (minimumRequirements.ContainsKey(itemId))
                {
                    throw new InvalidOperationException(
                        $"BridgeIndex {bridgeIndex}에 중복된 ItemId가 있습니다: {itemId}");
                }

                minimumRequirements.Add(itemId, minimumCount);
            }

            return minimumRequirements;
        }
    }
}
