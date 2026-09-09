using HelloServer;
using UnityEngine;

namespace WeGotOne
{
    public class LandingZoneTrigger : MonoBehaviour
    {
        // 물리적인 충돌 감지는 실제 트리거 오브젝트가 직접 수행합니다 [1]
        private void OnTriggerEnter2D(Collider2D other)
        {
            
            // 부딪힌 대상이 플레이어인지 확인합니다
            PlayerController player = other.GetComponentInParent<PlayerController>();
            if (TutorialManager.Instance.CurrentStep >= TutorialManager.TutorialStep.StormActive && 
                (other.CompareTag("Player") || (player != null && player.User.Id == RoomClient.Instance.Me.Id)))
            {
                // 싱글톤인 TutorialManager에게 "플레이어 착지 완료!"라고 신호를 보냅니다.
                if (TutorialManager.Instance != null )
                {
                    TutorialManager.Instance.OnPlayerLanded();
                    
                    Debug.Log("튜토리얼 끝");
                    
                    // 한 번 작동한 트리거는 중복 작동하지 않도록 스스로 비활성화합니다.
                    gameObject.SetActive(false); 
                }
            }
        }
    }
}