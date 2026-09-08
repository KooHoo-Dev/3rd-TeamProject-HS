using HelloServer;
using UnityEngine;

namespace WeGotOne
{
    // ==========================================
    // [수중 지뢰 넉백 트리거] - 신규
    // 플레이어의 신체 일부가 지뢰 콜라이더에 닿으면, 몸통(Body)에
    // 지뢰 중심 기준 반대 방향으로 힘을 가해 몸 전체를 날려보냅니다.
    // 물리 판정은 호스트에서만 수행하고, 게스트는 다음 스냅샷을 통해
    // 결과 좌표를 그대로 따라갑니다. 쿨타임이 지나면 재사용 가능합니다.
    // ==========================================
    public class MineController : MonoBehaviour
    {
        [Header("넉백 설정")]
        [SerializeField] private float knockbackForce = 15f;

        [Header("재사용 쿨타임")]
        [SerializeField] private float cooldown = 1.0f;
        private float lastTriggeredTime = -999f;

        private bool IsHost => RoomClient.Instance != null && 
                                                      RoomClient.Instance.Me != null && 
                                                      RoomClient.Instance.Me.IsHost;

        
        private void OnCollisionEnter2D(Collision2D collision)
        {
            
            // 물리 판정은 호스트 전용 - 게스트는 스냅샷으로 결과만 반영받음
            if (IsHost == false) return;
            if (Time.time - lastTriggeredTime < cooldown) return;

            // 닿은 콜라이더가 어느 플레이어 소속인지 확인 (물고기/판자 등은 무시)
            PlayerController player = collision.gameObject.GetComponentInParent<PlayerController>();
            if (player == null) return;
            
            // Rigidbody2D 부품을 가장 안전하게 검색해옵니다.
            Rigidbody2D bodyRb = player.GetComponentInChildren<Rigidbody2D>();
            if (bodyRb == null)
            {
                
                return;
            }
            
            // 3. 실제 물리 엔진이 달린 bodyRb의 좌표를 기준으로 넉백 방향을 계산합니다.
            Vector2 direction = (bodyRb.position - (Vector2)transform.position).normalized;
            if (direction == Vector2.zero) direction = Vector2.up; // 완전히 겹친 경우 방어
            
            // 4. 찾은 Rigidbody2D에 힘을 즉각 가합니다.
            bodyRb.linearVelocity = new Vector2(direction.x * knockbackForce, direction.y * knockbackForce);

            lastTriggeredTime = Time.time;
            
        }
    }
}