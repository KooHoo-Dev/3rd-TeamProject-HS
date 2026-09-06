using HelloServer;
using UnityEngine;

namespace WeGotOne
{
    public class LandingZoneTrigger : MonoBehaviour
    {
        // 물리적인 충돌 감지는 실제 트리거 오브젝트가 직접 수행합니다 [1]
        private void OnTriggerEnter2D(Collider2D other)
        {
            // 물리 판정은 호스트 컴퓨터에서만 수행하도록 제한합니다
            if (RoomClient.Instance == null || !RoomClient.Instance.Me.IsHost) return;

            // 부딪힌 대상이 플레이어인지 확인합니다
            if (other.CompareTag("Player") || other.GetComponentInParent<PlayerController>() != null)
            {
                // 싱글톤인 TutorialManager에게 "플레이어 착지 완료!"라고 신호를 보냅니다.
                if (TutorialManager.Instance != null)
                {
                    TutorialManager.Instance.OnPlayerLanded();
                    
                    // 한 번 작동한 트리거는 중복 작동하지 않도록 스스로 비활성화합니다.
                    gameObject.SetActive(false); 
                }
            }
        }
    }
}