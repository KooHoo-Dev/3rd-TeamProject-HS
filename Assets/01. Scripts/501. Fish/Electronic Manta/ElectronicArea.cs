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
                PlayerParalysisAsync(other).Forget();
                StartCoroutine(ElectronicAttackActive());
            }
        }
        
        private async UniTaskVoid PlayerParalysisAsync(Collider2D other)
        {
            PlayerController player = other.GetComponentInParent<PlayerController>();
            if (player == null) return;

            player.IsParalysis = true;

            // 오브젝트 비활성화와 무관하게 동작하며, Time.timeScale의 영향도 정상적으로 받습니다.
            // player가 도중에 파괴되면 작업이 안전하게 취소되도록 토큰을 연결합니다.
            await UniTask.Delay(
                System.TimeSpan.FromSeconds(paralysisDuration), 
                cancellationToken: player.destroyCancellationToken
            );

            if (player != null)
            {
                player.IsParalysis = false;
                Debug.Log(player.IsParalysis);
            }
        }
        
        private IEnumerator ElectronicAttackActive()
        {
            Debug.Log("Attack false");
            if(electronicParticle)
                electronicParticle.SetActive(false);
            isAttack = false;

            yield return new WaitForSeconds(attackFalseDuration);

            if(electronicParticle)
                electronicParticle.gameObject.SetActive(true);
            isAttack = true;
        }
        
    }
}