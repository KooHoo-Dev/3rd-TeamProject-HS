using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace WeGotOne
{
    public class BridgeManager : SingletonBase<BridgeManager>
    { 
        [field: SerializeField] public GameObject[] BridgeObjects { get; private set; }
        private Inventory targetInventory;
        
        public bool IsEnd => CurrentBridgeCount() == BridgeObjects.Length;
        
        // 게임 매니저나 나중에 다른 UI에게 전파하는 용도 델리게이트
        public event Action OnBridgeBuild;

        protected override void Awake()
        {
            targetInventory = Inventory.Instance;
        }

        public int CurrentBridgeCount()
        {
            int currentCount = 0;

            for (int i = 0; i < BridgeObjects.Length; i++)
            {
                if (BridgeObjects[i].activeSelf)
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
                        BridgeObjects[CurrentBridgeCount()].SetActive(true);
                        OnBridgeBuild?.Invoke();
                        return;
                    }
                }
            }
        }
        
    }
}