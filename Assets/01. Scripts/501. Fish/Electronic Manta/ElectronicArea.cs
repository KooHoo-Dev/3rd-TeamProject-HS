using System;
using System.Threading;
using Cysharp.Threading.Tasks;
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
                PlayerParalysis(other, other.GetCancellationTokenOnDestroy()).Forget();
            }
        }

        // duration 초 단위로 시간을 받습니다.
        public async UniTask PlayerParalysis(Collider2D other, CancellationToken token)
        {
            PlayerController player = other.GetComponent<PlayerController>();
            
            player.IsParalysis = true;

            // 지정된 시간만큼 대기 (이 동안 다른 프레임 진행)
            await UniTask.Delay(System.TimeSpan.FromSeconds(duration), cancellationToken: token);

            player.IsParalysis = false;
        }
        
    }
}