using System.Collections;
using Cysharp.Threading.Tasks;
using UnityEngine;
using WeGotOne.Fish_Interface;

namespace WeGotOne.Electronic_Manta
{
    public class ElectronicArea : MonoBehaviour
    {
        [SerializeField] private float paralysisDuration;
        [SerializeField] private float attackFalseDuration;
        [SerializeField] private FishStateManager stateManager;
        [SerializeField] private GameObject electronicParticle;

        [SerializeField] private bool isAttack = true;
        
        private void OnEnable()
        {
            stateManager = GetComponentInParent<FishStateManager>();
            isAttack = true;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Player") && isAttack)
            {
                Debug.Log("Paralysis Enter");
                StartCoroutine(PlayerParalysis(other));
                StartCoroutine(ElectronicAttackActive());
            }
        }
        
        private IEnumerator PlayerParalysis(Collider2D other)
        {
            PlayerController player = other.GetComponentInParent<PlayerController>();
            
            player.IsParalysis = true;
            
            yield return new WaitForSeconds(paralysisDuration);

            player.IsParalysis = false;
            Debug.Log(player.IsParalysis);
        }
        
        private IEnumerator ElectronicAttackActive()
        {
            Debug.Log("Attack false");
            electronicParticle.SetActive(false);
            isAttack = false;

            yield return new WaitForSeconds(attackFalseDuration);

            Debug.Log("Attack true");
            electronicParticle.gameObject.SetActive(true);
            isAttack = true;
        }
        
    }
}