using System;
using System.Collections.Generic;
using HelloServer;
using UnityEngine;

namespace WeGotOne
{
    public class InventoryItemDB : SingletonBase<InventoryItemDB>
    {
        private const int DEFAULT_ITEM_KEY = 0;

        public ItemScriptableObject[] items;

        private readonly Dictionary<int, ItemScriptableObject> itemDic = new();
        private readonly Dictionary<int, int> itemCountDic = new();

        public event Action<int, int> ItemCountChanged;

        public IEnumerable<KeyValuePair<int, int>> EnumerateCounts()
        {
            return itemCountDic;
        }

        // 제공해드린 싱글톤을 상속받은 개체의 초기화는
        // OnInitialize(), Start()를 중심으로 하되
        // Awake 꼭 필요한 상황에서는 override를 하시고
        // base.Awake()를 호출해 주세요

        protected override void OnInitialize()
        {
            base.OnInitialize();

            if (items == null)
            {
                Debug.LogWarning("[InventoryItemDB] items가 설정되지 않았습니다.");
                return;
            }

            for (int i = 0; i < items.Length; ++i)
            {
                ItemScriptableObject item = items[i];
                if (item == null)
                {
                    Debug.LogWarning($"[InventoryItemDB] items[{i}]가 비어 있습니다.");
                    continue;
                }

                if (itemDic.ContainsKey(item.Key))
                {
                    Debug.LogWarning($"[InventoryItemDB] 중복 ItemId = {item.Key}");
                    continue;
                }

                itemDic.Add(item.Key, item);
                itemCountDic.Add(item.Key, 0);
            }
        }

        // 견본(ScriptableObject 참조변수)를 반환하는 함수입니다.
        // - 지금은 단순 참조로 충분하므로 불필요한 인스턴스 생성을 피합니다.
        public ItemScriptableObject GetItem(int id)
        {
            if (itemDic.TryGetValue(id, out ItemScriptableObject item))
            {
                return item;
            }

            Debug.LogWarning($"[InventoryItemDB] 없는 Key = {id}");
            return itemDic.TryGetValue(DEFAULT_ITEM_KEY, out ItemScriptableObject defaultItem)
                ? defaultItem
                : null;
        }

        public int GetCount(int itemId)
        {
            return itemCountDic.TryGetValue(itemId, out int count) ? count : 0;
        }

        public bool Add(int itemId, int amount)
        {
            if (amount <= 0)
            {
                Debug.LogWarning($"[InventoryItemDB] 추가 수량은 양수여야 합니다. ItemId = {itemId}, Amount = {amount}");
                return false;
            }

            if (itemCountDic.TryGetValue(itemId, out int currentCount) == false)
            {
                Debug.LogWarning($"[InventoryItemDB] 없는 ItemId에 추가 시도 = {itemId}");
                return false;
            }

            SetCount(itemId, currentCount + amount);
            return true;
        }

        public bool TryConsume(int itemId, int amount)
        {
            if (amount <= 0)
            {
                Debug.LogWarning($"[InventoryItemDB] 소비 수량은 양수여야 합니다. ItemId = {itemId}, Amount = {amount}");
                return false;
            }

            if (itemCountDic.TryGetValue(itemId, out int currentCount) == false)
            {
                Debug.LogWarning($"[InventoryItemDB] 없는 ItemId에 소비 시도 = {itemId}");
                return false;
            }

            if (currentCount < amount)
            {
                return false;
            }

            SetCount(itemId, currentCount - amount);
            return true;
        }

        public void ApplySnapshot(InventoryEntryState[] entries)
        {
            Dictionary<int, int> snapshotCounts = new();

            for (int i = 0; i < entries.Length; ++i)
            {
                InventoryEntryState entry = entries[i];

                if (snapshotCounts.ContainsKey(entry.ItemId))
                {
                    Debug.LogWarning($"[InventoryItemDB] 중복 ItemId 스냅샷 = {entry.ItemId}");
                    continue;
                }

                snapshotCounts.Add(entry.ItemId, entry.Count);
            }

            List<int> itemIds = new(itemCountDic.Keys);
            for (int i = 0; i < itemIds.Count; ++i)
            {
                int itemId = itemIds[i];
                int count = snapshotCounts.TryGetValue(itemId, out int snapshotCount)
                    ? snapshotCount
                    : 0;

                SetCount(itemId, count);
            }
        }

        private void SetCount(int itemId, int newCount)
        {
            if (itemCountDic[itemId] == newCount)
            {
                return;
            }

            itemCountDic[itemId] = newCount;
            ItemCountChanged?.Invoke(itemId, newCount);
        }
    }
}

