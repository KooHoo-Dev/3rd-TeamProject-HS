using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace WeGotOne
{
    public class BridgeManager : SingletonBase<BridgeManager>
    { 
        [field: SerializeField] public WoodBridge[] BridgeObjects { get; private set; }
        private Inventory targetInventory;
        
        public bool IsEnd => CurrentBridgeCount() == BridgeObjects.Length;
        
        // 게임 매니저나 나중에 다른 UI에게 전파하는 용도 델리게이트
        public event Action OnBridgeBuild;

        // Awake -> OnInitialize 로 전환 (SingletonBase)에 맞게
        protected override void OnInitialize()
        {
            base.OnInitialize();
            targetInventory = Inventory.Instance;
            BridgeObjects = GetComponentsInChildren<WoodBridge>(true);
        }

        public int CurrentBridgeCount()
        {
            int currentCount = 0;

            for (int i = 0; i < BridgeObjects.Length; i++)
            {
                if (BridgeObjects[i].gameObject.activeSelf)
                {
                    currentCount++;
                }
            }

            return currentCount;
        }
        
        public void BridgeBuild()
        {
            InventorySlot[,] grid = targetInventory.SlotGrid;

            for (int i = 0; i < targetInventory.SizeRow; i++)
            {
                for (int j = 0; j < targetInventory.SizeColumn; j++)
                {
                    if (grid[i, j].Item.ItemType == ItemType.Plank && grid[i, j].ItemCount > 0)
                    {
                        grid[i, j].CountTextUpdate(false);
                        BridgeObjects[CurrentBridgeCount()].gameObject.SetActive(true);
                        OnBridgeBuild?.Invoke();
                        return;
                    }
                }
            }
        }

        #region test methods

        // 테스트 메소드 (나무 비용 없이)
        public void BuildBridgeWithoutCost()
        {
            BridgeObjects[CurrentBridgeCount()].gameObject.SetActive(true);
            OnBridgeBuild?.Invoke();
        }

        #endregion
        
    }
}