using UnityEngine;
using UnityEngine.Serialization;

namespace WeGotOne
{
    public class BridgeManager : SingletonBase<BridgeManager>
    { 
        [field: SerializeField]
        public GameObject[] BridgeObjects { get; private set; }
        
        private Inventory targetInventory;
        
        public bool IsEnd => CurrentBridgeCount() == BridgeObjects.Length;

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
                        if (IsEnd) return;
                        
                        grid[i, j].CountTextUpdate(false);
                        BridgeObjects[CurrentBridgeCount()].SetActive(true);
                        Debug.Log(CurrentBridgeCount());
                        return;
                    }
                }
            }
        }
        
    }
}