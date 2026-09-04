using System;
using System.Threading;
using System.Threading.Tasks;
using DG.Tweening;
using HelloServer;
using UnityEngine;
using Random = UnityEngine.Random;

namespace WeGotOne
{
    public class FishMovement : MonoBehaviour
    {
        [SerializeField] private Collider2D waterArea;
        public Collider2D WaterArea => waterArea;
        
        public Rigidbody2D Rb { get; private set; }
        private Vector2 currentDirection;
        private float currentSpeed;
        public bool isGuestLocked;
        private Vector3 baseLocalScale;
        private LayerMask waterLayer;

        private void Awake()
        {
            Rb = GetComponent<Rigidbody2D>();
            baseLocalScale = transform.localScale;
            waterLayer = LayerMask.GetMask("Water");
        }
        
        private void FixedUpdate()
        {

            if (CheckInWater() == false) return;
            
            // Move 명령이 들어왔을 때만 지속적인 속도를 적용
            if (currentSpeed > 0)
            {
                Rb.linearVelocity = currentDirection * currentSpeed;
                FlipRenderer(currentDirection.x);
                Debug.Log(currentSpeed);
                Debug.Log(Rb.name);
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (RoomClient.Instance.Me.IsHost && waterLayer.Contains(other.gameObject.layer))
            {
                Rb.gravityScale = 0f;
            }
        }
        
        private void OnTriggerExit2D(Collider2D other)
        {
            //
            // 특정 단일 레이어인지 검사
            if (RoomClient.Instance.Me.IsHost && waterLayer.Contains(other.gameObject.layer))
            {
                Rb.gravityScale = 1f;
            }
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
            if (Rb != null)
            {
                Rb.AddForce(forceVector, ForceMode2D.Force);
            }
        }

        // 3. 이동 정지
        public void StopMove()
        {
            currentSpeed = 0f;
            currentDirection = Vector2.zero;
            if (Rb != null) Rb.linearVelocity = Vector2.zero;
        }

        // 4. 기절 시 관성 미끄러짐 
        public void StartFaintDrift(float duration)
        {
            StopMove();
            // 필요에 따라 drag(마찰력) 값을 조절해 자연스럽게 미끄러지다 멈추게 연출
            if (Rb != null) Rb.linearDamping = 1.5f; 
        }

        // 이동 방향에 맞춰 스프라이트 좌우 반전
        private void FlipRenderer(float xDir)
        {
            if (Mathf.Abs(xDir) > 0.01f)
            {
                transform.localScale = new Vector3(Mathf.Sign(-xDir) * baseLocalScale.x, baseLocalScale.y, baseLocalScale.z);
            }
        }

        // 물 안/밖 물리 속성 (기존 유지)
        public void SetAirPhysics()
        {
            if (Rb != null) { Rb.gravityScale = 1f; Rb.linearDamping = 0f; }
        }
        
        public void SetWaterPhysics()
        {
            if (Rb != null) { Rb.gravityScale = 0f; Rb.linearDamping = 3f; }
        }

        private bool CheckInWater()
        {
            Vector2 next = Rb.position + currentDirection * currentSpeed * Time.fixedDeltaTime;
            
            // 움직임 구간(물영역)이 정해져있고,
            if (waterArea != null)
            {
                // 다음 위치가 물이랑 겹쳐있지 않으면
                if (waterArea.OverlapPoint(next) == false)
                {
                    // 속도 없애고 false
                    Rb.linearVelocity = Vector2.zero;
                    return false;
                }
            }

            // 아니면 가능
            return true;
        }
        
        public void SetWaterArea(Collider2D water)
        {
            waterArea = water;
        }

        public void LockForGuest()
        {
            isGuestLocked = true;

            StopMove(); // 혹시 이미 걸려있던 트윈이 있다면 즉시 정리

            // [요구사항 2] Rigidbody Dynamic -> Kinematic 전환
            if (Rb != null) Rb.bodyType = RigidbodyType2D.Kinematic;

            enabled = false; // Update 등 자동 콜백까지 이중 차단
        }
    }
}