using System;
using _99._KimSeungGu;
using Study.Utilities;
using UnityEngine;
using UnityEngine.InputSystem;

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

        private void OnTriggerStay2D(Collider2D other)
        {
            if (other.CompareTag("Player"))
            {
                if (SimpleInput.GetKeyDown(Key.F))
                {
                    InventorySlot[,] grid = targetInventory.SlotGrid;

                    for (int i = 0; i < targetInventory.SizeRow; i++)
                    {
                        for (int j = 0; j < targetInventory.SizeColumn; j++)
                        {
                            if (grid[i, j].Item.ItemType == ItemType.Plank && grid[i, j].ItemCount > 0)
                            {
                                grid[i, j].CountTextUpdate(false);
                                BridgeManager.Instance.BridgeConstruction();
                            }
                        }
                    }
                }
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.tag is "Fish" or "Item")
            {
                InventorySlot[,] grid = targetInventory.SlotGrid;

                InventoryItemDB db = InventoryItemDB.Instance;

                if (other.TryGetComponent<IScriptableObjectKind>(out var holder))
                {
                    ItemScriptableObject item = holder.ThisItem;

                    if (item == null) return;

                    for (int i = 0; i < targetInventory.SizeRow; ++i) // 여기 I(아이)임
                    {
                        for (int j = 0; j < targetInventory.SizeColumn; ++j) // 여기 J(제이)임
                        {
                            if (grid[i, j].IsEmpty)
                            {
                                grid[i, j].SetItem(db.GetItem(item.Key));
                                grid[i, j].CountTextUpdate(true);
                                holder.OnPickup();
                                return;
                            }

                            if (grid[i, j].Item.Name == item.Name)
                            {
                                grid[i, j].CountTextUpdate(true);
                                holder.OnPickup();
                                return;
                            }
                        }
                    }
                }   
            }
        }
    }
}