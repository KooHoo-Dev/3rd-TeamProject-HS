using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Study_Inventory
{
    // 다리
    public class InventoryDebugger : MonoBehaviour
    {
        public Inventory targetInventory;

        private void Awake()
        {
            targetInventory = Inventory.Instance;
        }

        private void OnTriggerEnter(Collider other)
        {
            InventoryItem item = other.GetComponent<InventoryItem>();
            
            InventorySlot[,] grid = targetInventory.SlotGrid;

            InventoryItemDB db = InventoryItemDB.Instance;

            for (int i = 0; i  < targetInventory.SizeRow; ++i) // 여기 I(아이)임
            {
                for(int j = 0; j < targetInventory.SizeColumn; ++j) // 여기 J(제이)임
                {
                    if (grid[i,j].IsEmpty)
                    {
                        grid[i, j].SetItem(db.GetItem(item.Key));
                        other.gameObject.SetActive(false);
                        break;
                    }
                }
            }
        }
    }

}

