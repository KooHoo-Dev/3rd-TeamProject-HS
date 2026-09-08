using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace WeGotOne
{
    public class BridgeRequirementUI : MonoBehaviour
    {
        [SerializeField] private InventorySlot[] requirementSlots;
        [SerializeField] private TMP_Text buildValueText;

        private readonly List<KeyValuePair<int, int>> sortedRequirements = new();

        private InventoryItemDB inventoryItemDB;
        private BridgeManager bridgeManager;

        private void Awake()
        {
            requirementSlots = GetComponentsInChildren<InventorySlot>();
        }

        private IEnumerator Start()
        {
            yield return new WaitUntil(() => InventoryItemDB.Instance.IsInitialized && BridgeManager.IsInitialized && ItemManager.Instance.IsInitialized);

            inventoryItemDB = InventoryItemDB.Instance;
            bridgeManager = BridgeManager.Instance;

            inventoryItemDB.ItemCountChanged += HandleItemCountChanged;
            bridgeManager.OnBridgeBuild += Refresh;

            yield return new WaitUntil(() =>
                bridgeManager.TryGetNextBridgeRef(out _) || bridgeManager.IsEnd);

            Refresh();
        }

        private void OnDestroy()
        {
            if (inventoryItemDB != null)
            {
                inventoryItemDB.ItemCountChanged -= HandleItemCountChanged;
            }

            if (bridgeManager != null)
            {
                bridgeManager.OnBridgeBuild -= Refresh;
            }
        }

        private void HandleItemCountChanged(int itemId, int count)
        {
            Refresh();
        }

        private void Refresh()
        {
            if (bridgeManager.TryGetNextBridgeRef(out BridgeRef bridgeRef) == false)
            {
                ClearRequirementSlots();
                buildValueText.text = string.Empty;
                return;
            }

            RenderMinimumRequirements(bridgeRef);
            RenderBuildValue(bridgeRef);
        }

        private void RenderMinimumRequirements(BridgeRef bridgeRef)
        {
            sortedRequirements.Clear();
            foreach (KeyValuePair<int, int> requirement in bridgeRef.MinimumRequirements)
            {
                sortedRequirements.Add(requirement);
            }

            sortedRequirements.Sort((left, right) => left.Key.CompareTo(right.Key));

            for (int i = 0; i < requirementSlots.Length; i++)
            {
                InventorySlot slot = requirementSlots[i];
                if (slot == null)
                {
                    continue;
                }

                if (i >= sortedRequirements.Count)
                {
                    slot.Render(null, 0);
                    continue;
                }

                KeyValuePair<int, int> requirement = sortedRequirements[i];
                ItemRef item = inventoryItemDB.GetItem(requirement.Key);

                int currentCount = inventoryItemDB.GetCount(requirement.Key);
                slot.Render(item, requirement.Value);
                slot.ItemCountText.text = $"{currentCount}/{requirement.Value}";
            }
        }

        private void RenderBuildValue(BridgeRef bridgeRef)
        {
            long currentBuildValue = 0;

            foreach (KeyValuePair<int, int> inventoryEntry in inventoryItemDB.EnumerateCounts())
            {
                if (inventoryEntry.Value <= 0)
                {
                    continue;
                }

                ItemRef item = inventoryItemDB.GetItem(inventoryEntry.Key);
                if (item.BuildValue > 0)
                {
                    currentBuildValue += (long)item.BuildValue * inventoryEntry.Value;
                }
            }

            buildValueText.text = $"{currentBuildValue} / {bridgeRef.RequiredValue}";
        }

        private void ClearRequirementSlots()
        {
            for (int i = 0; i < requirementSlots.Length; i++)
            {
                if (requirementSlots[i] != null)
                {
                    requirementSlots[i].Render(null, 0);
                }
            }
        }
    }
}
