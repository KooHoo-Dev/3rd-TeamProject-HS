using System;
using System.Threading;
using System.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using Random = UnityEngine.Random;

namespace WeGotOne
{
    public class FishMovement : MonoBehaviour
    {
        private Rigidbody2D rb;
        private Vector2 currentDirection;
        private float currentSpeed;

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
        }

        // 1. 일반 이동 (Patrol, Runaway 등에서 지속 호출)
        public void Move(Vector2 direction, float speed)
        {
            currentDirection = direction.normalized;
            currentSpeed = speed;
        }

        // 2. 물리적 힘 가하기 (Caught 상태의 발버둥 등에서 단발성 호출)
        public void ApplyForce(Vector2 forceVector)
        {
            if (rb != null)
            {
                rb.AddForce(forceVector, ForceMode2D.Force);
            }
        }

        // 3. 이동 정지
        public void StopMove()
        {
            currentSpeed = 0f;
            currentDirection = Vector2.zero;
            if (rb != null) rb.velocity = Vector2.zero;
        }

        // 4. 기절 시 관성 미끄러짐 
        public void StartFaintDrift(float duration)
        {
            StopMove();
            // 필요에 따라 drag(마찰력) 값을 조절해 자연스럽게 미끄러지다 멈추게 연출
            if (rb != null) rb.drag = 1.5f; 
        }

        private void FixedUpdate()
        {
            // Move 명령이 들어왔을 때만 지속적인 속도를 적용
            if (currentSpeed > 0)
            {
                rb.velocity = currentDirection * currentSpeed;
                FlipRenderer(currentDirection.x);
            }
        }

        // 이동 방향에 맞춰 스프라이트 좌우 반전
        private void FlipRenderer(float xDir)
        {
            if (Mathf.Abs(xDir) > 0.01f)
            {
                transform.localScale = new Vector3(Mathf.Sign(xDir), 1, 1);
            }
        }

        // 물 안/밖 물리 속성 (기존 유지)
        public void SetAirPhysics()
        {
            if (rb != null) { rb.gravityScale = 1f; rb.drag = 0f; }
        }
        
        public void SetWaterPhysics()
        {
            if (rb != null) { rb.gravityScale = 0f; rb.drag = 3f; }
        }
    }
}