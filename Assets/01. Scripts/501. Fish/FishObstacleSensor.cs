using System;
using UnityEngine;

namespace WeGotOne
{
    /// <summary>
    /// 물고기 진행 방향의 장애물(벽, 다른 물고기 등)을 감지하고 알리는 센서 컴포넌트입니다.
    /// </summary>
    [RequireComponent(typeof(FishMovement))]
    public class FishObstacleSensor : MonoBehaviour
    {
        [Header("장애물 감지 설정")]
        [SerializeField] private LayerMask obstacleLayer;
        [SerializeField] private Vector2 obstacleCheckSize = new Vector2(0.8f, 0.5f);
        [SerializeField] private float obstacleCheckDistance = 1.0f;

        private FishMovement movement;

        // 장애물이 감지되었을 때 외부에 알릴 이벤트
        public event Action OnObstacleDetected;

        private void Awake()
        {
            movement = GetComponent<FishMovement>();
        }

        private void Update()
        {
            if (movement == null) return;

            // 물고기가 순찰 중일 때만 실시간으로 장애물 체크를 실행합니다.
            if (movement.IsPatrolling)
            {
                CheckForObstacles();
            }
        }

        private void CheckForObstacles()
        {
            // 물고기의 현재 목표 지점 방향으로 BoxCast를 발사합니다.
            Vector3 targetPos = movement.TargetPosition;
            Vector2 moveDirection = (targetPos - transform.position).normalized;

            RaycastHit2D hit = Physics2D.BoxCast(
                transform.position,
                obstacleCheckSize,
                0f,
                moveDirection,
                obstacleCheckDistance,
                obstacleLayer
            );

            if (hit.collider != null)
            {
                // 장애물이 잡히면 "나 앞에 뭐 부딪혔어!"라고 외칩니다.
                OnObstacleDetected?.Invoke();
            }
        }

        // 디버깅용 기즈모 드로잉 코드가 이리로 이사 왔습니다.
        private void OnDrawGizmosSelected()
        {
            if (movement == null) movement = GetComponent<FishMovement>();
            if (movement == null) return;

            Gizmos.color = Color.red;
            Vector3 targetPos = movement.TargetPosition;
            Vector3 dir = (targetPos - transform.position).normalized;

            // 시작 위치 박스
            Gizmos.DrawWireCube(transform.position, new Vector3(obstacleCheckSize.x, obstacleCheckSize.y, 0f));
            
            // 끝 위치 박스
            Vector3 endPosition = transform.position + dir * obstacleCheckDistance;
            Gizmos.DrawWireCube(endPosition, new Vector3(obstacleCheckSize.x, obstacleCheckSize.y, 0f));

            // 연결 선
            Gizmos.DrawLine(transform.position, endPosition);
        }
    }
}