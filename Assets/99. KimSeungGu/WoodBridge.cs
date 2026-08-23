using System;
using _99._KimSeungGu;
using UnityEngine;

namespace Study_Inventory
{
    // 다리
    public class WoodBridge : MonoBehaviour
    {
        public Inventory targetInventory;

        private void Awake()
        {
            targetInventory = Inventory.Instance;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            Debug.Log(other.gameObject.name);
            
            ItemScriptableObject item = other.GetComponent<Fish>().ThisItem;
            
            InventorySlot[,] grid = targetInventory.SlotGrid;

            InventoryItemDB db = InventoryItemDB.Instance;

            if (item.ItemType == ItemType.Fish)
            {
                for (int i = 0; i  < targetInventory.SizeRow; ++i) // 여기 I(아이)임
                {
                    for(int j = 0; j < targetInventory.SizeColumn; ++j) // 여기 J(제이)임
                    {
                        if (grid[i,j].IsEmpty)
                        {
                            grid[i, j].SetItem(db.GetItem(item.Key));
                            other.gameObject.SetActive(false);
                            return;
                        }
                    }
                }
            }
        }
    }

}

