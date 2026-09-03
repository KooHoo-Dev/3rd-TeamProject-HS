using System.Collections;
using UnityEngine;
using WeGotOne.Fish_Interface;

namespace WeGotOne.Electronic_Manta
{
    public class ElectronicArea : MonoBehaviour
    {
        [SerializeField] private float duration;
        [SerializeField] private FishStateManager stateManager;

        private void OnEnable()
        {
            stateManager = GetComponentInParent<FishStateManager>();
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Player") && stateManager.IsAttack)
            {
                Debug.Log("Paralysis Enter");
                StartCoroutine(PlayerParalysis(other));
            }
        }
        
        private IEnumerator PlayerParalysis(Collider2D other)
        {
            PlayerController player = other.GetComponentInParent<PlayerController>();
            
            player.IsParalysis = true;

            yield return new WaitForSeconds(duration);

            player.IsParalysis = false;
        }
        
    }
}