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
                BridgeManager.Instance.HandleItemPickup(other);
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
