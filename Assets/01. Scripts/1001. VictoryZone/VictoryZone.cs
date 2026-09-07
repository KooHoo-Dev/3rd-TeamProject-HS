using Unity.VisualScripting;
using UnityEngine;
using HelloServer;

namespace WeGotOne
{
    public class VictoryZone : MonoBehaviour
    {
        private void OnTriggerStay2D(Collider2D other)
        {
            if (CanRunLocalGameLogic == false) return;

            if (other.CompareTag("Player"))
            {
                PlayerController playerController = other.GetComponentInParent<PlayerController>();
                if (playerController == null || playerController.User == null) return;

                RoomClient.Instance.SetPlayerArrived(playerController.User.Id, true);
                GameManager.Instance.AddArrivedPlayerCount(playerController.User.Id);
            }
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (CanRunLocalGameLogic == false) return;

            if (other.CompareTag("Player"))
            {
                PlayerController playerController = other.GetComponentInParent<PlayerController>();
                if (playerController == null || playerController.User == null) return;

                RoomClient.Instance?.SetPlayerArrived(playerController.User.Id, false);
                GameManager.Instance?.RemoveArrivedPlayerCount(playerController.User.Id);
            }
        }

        private bool CanRunLocalGameLogic
        {
            get
            {
                return RoomClient.IsInitialized == false ||
                       RoomClient.Instance.Me == null ||
                       RoomClient.Instance.Me.IsHost;
            }
        }
    }
}
