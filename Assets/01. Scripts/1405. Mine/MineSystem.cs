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
        [SerializeField] private ForceMode2D forceMode = ForceMode2D.Impulse;

        [Header("재사용 쿨타임")]
        [SerializeField] private float cooldown = 1.0f;
        private float lastTriggeredTime = -999f;

        private bool isHost;

        private void Start()
        {
            isHost = RoomClient.Instance.Me != null && RoomClient.Instance.Me.IsHost;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            Debug.Log($"[Mine] 트리거 진입: {other.name}, isHost: {isHost}"); // 임시로 추가
            // 물리 판정은 호스트 전용 - 게스트는 스냅샷으로 결과만 반영받음
            if (!isHost) return;
            if (Time.time - lastTriggeredTime < cooldown) return;

            // 닿은 콜라이더가 어느 플레이어 소속인지 확인 (물고기/판자 등은 무시)
            PlayerController player = other.GetComponentInParent<PlayerController>();
            if (player == null) return;

            Rigidbody2D bodyRb = player.Body.GetComponent<Rigidbody2D>();
            if (bodyRb == null) return;

            Vector2 direction = (bodyRb.position - (Vector2)transform.position).normalized;
            if (direction == Vector2.zero) direction = Vector2.up; // 완전히 겹친 경우 방어

            bodyRb.AddForce(direction * knockbackForce, forceMode);

            lastTriggeredTime = Time.time;

            Debug.Log($"[Mine] {player.name} 몸통 넉백 발동 - 방향: {direction}");
        }
    }
}