using HelloServer;
using UnityEngine;
using UnityEngine.InputSystem;

namespace WeGotOne
{
    // 다리
    public class Bridge : MonoBehaviour
    {
        private Inventory targetInventory;

        private void Awake()
        {
            targetInventory = Inventory.Instance;
        }

        private void OnCollisionEnter2D(Collision2D other)
        {
            if (RoomClient.Instance.Me.IsHost == false) return;
            if (other.gameObject.tag is "Fish" or "Item")
            {
                InventorySlot[,] grid = targetInventory.SlotGrid;

                InventoryItemDB db = InventoryItemDB.Instance;

                if (other.gameObject.TryGetComponent<Collectible>(out var holder))
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
                                grid[i, j].CountTextUpdate(item.Key, true);
                                holder.OnPickup();
                                return;
                            }

                            if (grid[i, j].Item.Name == item.Name)
                            {
                                grid[i, j].CountTextUpdate(item.Key, true);
                                holder.OnPickup();
                                return;
                            }
                        }
                    }
                }   
            }
        }
        
        private void OnCollisionStay2D(Collision2D other)
        {
            if (RoomClient.Instance.Me.IsHost == false) return;
            if (other.gameObject.CompareTag("Player"))
            {
                PlayerController player = other.gameObject.GetComponentInParent<PlayerController>();
                if (player.IsBuild)
                {
                    BridgeManager.Instance.BridgeBuild();
                }
            }
        }

        // private void OnCollisionExit2D(Collision2D other)
        // {
        //     if (RoomClient.Instance.Me.IsHost == false) return;
        //     if (other.gameObject.CompareTag("Player"))
        //     {
        //         isCanBuild = false;
        //     }
        // }
    }
}