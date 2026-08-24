using Jay;
using Study_Inventory;
using Study.Utilities;
using UnityEngine;
using UnityEngine.InputSystem;

namespace _99._KimSeungGu
{
    public class BridgeManager : SingletonBase<BridgeManager>
    {
        [field: SerializeField] public GameObject[] BridgePrefab { get; private set; }
        private Inventory targetInventory;
        
        public bool IsEnd => CurrentBridgeCount() == BridgePrefab.Length;

        protected override void Awake()
        {
            targetInventory = Inventory.Instance;
        }

        public int CurrentBridgeCount()
        {
            int currentCount = 0;

            for (int i = 0; i < BridgePrefab.Length; i++)
            {
                if (BridgePrefab[i].activeSelf)
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
                        BridgePrefab[CurrentBridgeCount()].SetActive(true);
                        break;
                    }
                }
            }
        }
        
    }
}