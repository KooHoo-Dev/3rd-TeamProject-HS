using System;
using UnityEngine;
using WeGotOne.Fish_Interface;

namespace WeGotOne
{
    /// <summary>
    /// 물고기 진행 방향의 이동 가능 여부를 감지하고 알리는 센서 컴포넌트입니다.
    /// Water 밖 또는 장애물을 감지하면 이벤트를 발생시킵니다.
    /// </summary>
    [RequireComponent(typeof(FishMovement))]
    public class FishObstacleSensor : MonoBehaviour
    {
        [Header("물 영역 감지 설정")]
        [SerializeField] private LayerMask waterLayer;

        [Header("장애물 감지 설정")]
        [SerializeField] private LayerMask obstacleLayer;

        [Header("감지 범위 설정")]
        [SerializeField] private Vector2 obstacleCheckSize = new Vector2(0.8f, 0.5f);
        [SerializeField] private float obstacleCheckDistance = 1.0f;

        [Header("재감지 설정")]
        [SerializeField] private float detectCooldown = 0.1f;

        private float lastDetectedTime;

        private FishStateManager movement;

        // 이동할 수 없는 방향을 감지했을 때 알림
        public event Action<RaycastHit2D> OnMovementBlocked;

        private void Awake()
        {
            movement = GetComponent<FishStateManager>();
        }

        private void Update()
        {
            if (movement == null)
                return;

            CheckMovementPath();
        }

        private void CheckMovementPath()
        {
            Vector3 targetPos = movement.TargetPosition;

            Vector2 moveDirection =
                (targetPos - transform.position).normalized;

            if (moveDirection == Vector2.zero)
                return;

            RaycastHit2D obstacleHit = Physics2D.BoxCast(transform.position, obstacleCheckSize, 0f, 
                moveDirection, obstacleCheckDistance, obstacleLayer);

            bool hasObstacle = obstacleHit.collider != null;
            

            Vector2 checkPosition = (Vector2)transform.position + moveDirection * obstacleCheckDistance;

            Collider2D waterCollider = Physics2D.OverlapPoint(checkPosition, waterLayer);

            bool isOutsideWater = waterCollider == null;

            bool isBlocked = hasObstacle || isOutsideWater;

            if (isBlocked == false)
                return;

            if (Time.time - lastDetectedTime < detectCooldown)
                return;

            lastDetectedTime = Time.time;

            OnMovementBlocked?.Invoke(obstacleHit);
        }

        public bool IsObstacleAt(Vector2 position)
        {
            Vector2 castOffset = position - (Vector2)transform.position;
            RaycastHit2D obstacleHit = Physics2D.BoxCast(transform.position, obstacleCheckSize, 0f,
                castOffset.normalized, castOffset.magnitude, obstacleLayer);

            return obstacleHit.collider != null;
        }

        private void OnDrawGizmosSelected()
        {
            if (movement == null)
                movement = GetComponent<FishStateManager>();

            if (movement == null)
                return;

            Vector3 targetPos = movement.TargetPosition;
            Vector3 dir = (targetPos - transform.position).normalized;

            if (dir == Vector3.zero)
                return;
            
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(transform.position, new Vector3(obstacleCheckSize.x, obstacleCheckSize.y, 0f));
            Vector3 endPosition = transform.position + dir * obstacleCheckDistance;
            Gizmos.DrawWireCube(endPosition, new Vector3(obstacleCheckSize.x, obstacleCheckSize.y, 0f));
            Gizmos.DrawLine(transform.position, endPosition);
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(targetPos, 0.15f);
        }
    }
}
