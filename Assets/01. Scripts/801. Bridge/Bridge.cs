using HelloServer;
using UnityEngine;

namespace WeGotOne
{
    // 다리
    public class Bridge : MonoBehaviour
    {
        private void OnCollisionEnter2D(Collision2D other)
        {
            if (RoomClient.Instance.Me.IsHost == false) return;
            if (other.gameObject.tag is "Fish" or "Item")
            {
                if (other.gameObject.TryGetComponent<Collectible>(out var holder))
                {
                    ItemScriptableObject item = holder.ThisItem;

                    if (item == null) return;

                    if (InventoryItemDB.Instance.Add(item.Key, 1))
                    {
                        holder.OnPickup();
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
