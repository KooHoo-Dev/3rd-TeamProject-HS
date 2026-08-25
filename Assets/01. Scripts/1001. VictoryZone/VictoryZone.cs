using Unity.VisualScripting;
using UnityEngine;

namespace WeGotOne
{
    public class VictoryZone : MonoBehaviour
    {
        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Player"))
            {
                GameManager.Instance.AddArrivedPlayerCount();
            }
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (other.CompareTag("Player"))
            {
                GameManager.Instance.RemoveArrivedPlayerCount();
            }
        }
    }
}