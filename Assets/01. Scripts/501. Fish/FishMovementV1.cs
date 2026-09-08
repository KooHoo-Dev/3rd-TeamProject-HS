using System;
using System.Threading;
using System.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using Random = UnityEngine.Random;


namespace WeGotOne
{
    public class FishMovementV1 : MonoBehaviour
    {
        
        [SerializeField]
        [Tooltip("물고기의 이동속도: .SetSpeedBased 적용으로 거리와 무관하게 일정 속도 유지")]
        private float moveSpeed = 3f; // 물고기의 이동 속도

        [SerializeField]
        [Tooltip("물고기마다 개별적으로 가질 순찰 범위(좌우)")]
        private float patrolRange = 10f; // 기본값 10, 인스펙터에서 물고기마다 다르게 조절 가능
        
        [Header("도망(Runaway) 연출 설정")]
        [SerializeField]
        [Tooltip("도망갈 때의 속도 배율 (기존 순찰 속도 대비 몇 배 빠르게 달릴지)")]
        private float runawaySpeedMultiplier = 2.5f;

        [SerializeField]
        [Tooltip("도망치는 총 거리")]
        private float runawayDistance = 6.0f;

        [Header("힘겨루기(Struggle) 물리 설정")]
        [SerializeField] private float maxStruggleForce = 25f;         // 사방으로 발버둥치는 최대 물리 힘
        [SerializeField] private float directionChangeInterval = 0.3f;   // 도망치려는 방향을 바꾸는 주기 (초)
        
        [Header("장애물 감지 설정")]
        [SerializeField]
        [Tooltip("장애물로 인식할 레이어 (예: Wall, Fish 등)")]
        private LayerMask obstacleLayer;
        
        [SerializeField]
        [Tooltip("장애물을 감지할 전방 박스의 크기 (물고기 실제 콜라이더 크기와 맞추면 좋습니다)")]
        private Vector2 obstacleCheckSize = new Vector2(0.8f, 0.5f); // 가로 0.8, 세로 0.5 기본값
        
        [SerializeField]
        [Tooltip("장애물을 감지할 전방 레이 거리")]
        private float obstacleCheckDistance = 1.0f;
         
        private Animator anim;
        private Rigidbody2D rb; 
        
        private Tween patrolTween; // 순찰 트윈
        private Tween runawayTween;  // 도망 트윈
        private Tween driftTween; // 관성 감속용 트윈 (놓쳤을때 적용)
       
        private Vector3 spawnPosition; // 물고기의 시작(귀환) 지점, 
        private Vector3 patrolPosition; // 물고기의 순찰 포인트,
        private Vector2 struggleDirection;
        
        private float directionTimer;
        private float baseScaleX; // 물고기 스케일 절대 값
        
        private bool goingToPatrol; // 현재 patrolPoint 로 가는 중인지
        
        private void Awake()
        {   
            // 물고기의 생성 지점은 배치된 지점
            
            anim = GetComponent<Animator>();
            rb = GetComponent<Rigidbody2D>();
            baseScaleX = Mathf.Abs(transform.localScale.x);
        }

        private void Update()
        {
            // 현재 순찰 중이거나 회피 중일 때 실시간 장애물 체크 실행
            if (patrolTween != null && patrolTween.IsActive() && patrolTween.IsPlaying())
            {
                CheckForObstacles();
            }
        }
        
        

        /// <summary>
        /// 오브젝트 풀에서 꺼낼 때 호출할 초기화 함수
        /// </summary>
        /// <param name="startPos">스폰 위치</param>
       
        public void PullingInit(Vector3 startPos)
        {   
            // 물고기의 위치 세팅
            this.spawnPosition = startPos;
            transform.position = startPos;
            
            // 새로운 무작위 목표 지점을 정하고 순찰 시작
            CalculateNewPatrolRoute();
            StartPatrol();
            
            
        }
      
        
        /// <summary>
        /// 왕복 순찰 트윈을 시작합니다.
        /// </summary>
        private void StartPatrol()
        {
            StopMove();
            goingToPatrol = true;
            
            ApplyingFacing(patrolPosition.x - spawnPosition.x);

            // SetLoops(2, LoopType.Yoyo)를 통해 [출발지 -> 목적지 -> 출발지] 1회 왕복 후 정상 종료됨
            patrolTween = rb
                .DOMove(patrolPosition, moveSpeed)
                .SetSpeedBased() 
                .SetLoops(2, LoopType.Yoyo) 
                .SetEase(Ease.Linear)
                .OnStepComplete(HandleStepComplete) // 편도 도달 시(목적지 도착) 방향 전환
                .OnComplete(() =>
                {
                    // 왕복이 완벽하게 끝나 출발지로 돌아왔을 때, 다른 방향으로 경로를 재설정하여 무한 순찰
                    CalculateNewPatrolRoute();
                    StartPatrol();
                });
        }

       
        
        
        #region 이동 관련 함수 

        /// <summary>
        /// 물고기가 자연스럽게 대각선으로 도망치거나 발버둥 칠 방향 벡터를 반환합니다. (★중복 제거 및 공유)
        /// </summary>
        public Vector2 GetFleeDirection()
        {
            // X축은 좌우 랜덤, Y축은 너무 가파르지 않게 -0.3f ~ 0.3f 사이로 제한
            float fleeDirX = UnityEngine.Random.value > 0.5f ? 1f : -1f;
            float fleeDirY = UnityEngine.Random.Range(-0.3f, 0.3f); 
            return new Vector2(fleeDirX, fleeDirY).normalized;
        }

        /// <summary>
        /// 장애물 감지 즉시 현재 경로를 포기하고 반대 방향으로 즉시 복귀시킵니다.
        /// </summary>
        private void TurnAroundImmediately()
        {
            Debug.LogWarning($"{name} : 전방에 장애물 [{obstacleLayer.value}] 감지! 즉시 회피 및 반대 방향으로 복귀합니다.");
            
            StopMove();

            // 가던 방향 상태를 강제 토글
            goingToPatrol = !goingToPatrol;

            // 되돌아갈 대상 설정
            Vector3 escapeTarget = goingToPatrol ? patrolPosition : spawnPosition;
            ApplyingFacing(escapeTarget.x - transform.position.x);

            // 안전하게 원래 지점으로 되돌아가는 단방향 회피 트윈 실행
            patrolTween = rb.DOMove(escapeTarget, moveSpeed)
                .SetSpeedBased()
                .SetEase(Ease.Linear)
                .OnComplete(() =>
                {
                    // 무사히 복귀 지점에 도착했다면, 다시 다른 방향으로 새 왕복 경로 계산
                    CalculateNewPatrolRoute();
                    StartPatrol();
                });
        }
        
        /// <summary>
        /// 현재 시작 위치(spawnPosition)를 기준으로 새로운 대각선 순찰 지점을 계산합니다.
        /// </summary>
        private void CalculateNewPatrolRoute()
        {
            float randomDirectionX = Random.value > 0.5f ? 1f : -1f;
            float offsetX = randomDirectionX * patrolRange; 

            float verticalRange = patrolRange * Random.Range(0.2f, 0.5f);
            float randomDirectionY = Random.value > 0.5f ? 1f : -1f;
            float offsetY = randomDirectionY * verticalRange;

            this.patrolPosition = spawnPosition + new Vector3(offsetX, offsetY, 0f);
        }
        
        /// <summary>
        /// 플레이어를 비스듬히 피해 무작위 대각선 방향으로 빠르게 도망치는 비동기 메서드
        /// </summary>
        public async Task RunawayMoveAsync(CancellationToken token)
        {
            StopMove();
            runawayTween?.Kill();

            // [개선]: 기존 수식 대신 공용 헬퍼 함수를 호출해 일관성 있는 방향을 얻습니다.
            Vector3 fleeDirection = GetFleeDirection();

            Vector3 targetPosition = transform.position + fleeDirection * runawayDistance;
            ApplyingFacing(fleeDirection.x);

            float speed = moveSpeed * runawaySpeedMultiplier;

            runawayTween = rb.DOMove(targetPosition, speed)
                .SetSpeedBased()
                .SetEase(Ease.OutQuad);

            try
            {
                while (runawayTween != null && runawayTween.IsActive() && runawayTween.IsPlaying() && !token.IsCancellationRequested)
                {
                    await Task.Yield();
                }
            }
            finally
            {
                if (token.IsCancellationRequested)
                {
                    runawayTween?.Kill();
                    runawayTween = null;
                }
            }
        }
        
        /// <summary>
        /// 잡혀있던 반동/도망치던 관성을 그대로 유지한 채 지정된 시간 동안 서서히 감속하여 멈춥니다.
        /// </summary>
        /// <param name="duration">감속에 걸리는 시간 (초)</param>
        public void StartFaintDrift(float duration)
        {
            // 1. 기존의 순찰, 도망, 기존 감속 트윈만 골라 안전하게 제거합니다.
            // (이때 rb.linearVelocity를 0으로 밀지 않아 관성이 보존됩니다!)
            patrolTween?.Kill();
            runawayTween?.Kill();
            driftTween?.Kill();

            // 2. DOTween.To를 활용해 리지드바디의 '현재 물리 속도값'을 시작으로 
            // 서서히 Vector2.zero(정지) 상태로 이징(Ease)을 주며 감속시킵니다. [DoTween]
            // Ease.OutCubic 이징을 사용하면 처음엔 빠르게 미끄러지다 마찰력에 의해 부드럽게 서서히 멈추는 연출이 됩니다.
            driftTween = DOTween.To(
                () => rb.linearVelocity, 
                x => rb.linearVelocity = x, 
                Vector2.zero, 
                duration
            ).SetEase(Ease.OutCubic);

            // 3. 빙글빙글 도는 회전 속도 역시 동일한 주기로 서서히 정지시킵니다. [DoTween]
            DOTween.To(
                () => rb.angularVelocity,
                x => rb.angularVelocity = x,
                0f,
                duration
            ).SetEase(Ease.OutCubic);
        }
        
        public void StopMove()
        {
            if (patrolTween != null)
            {
                patrolTween.Kill();
                patrolTween = null;
            }
            if (runawayTween != null)
            {
                runawayTween.Kill();
                runawayTween = null;
            }
            if (driftTween != null)
            {
                driftTween.Kill();
                driftTween = null;
            }
            
            // [보완]: 선속도(linear)뿐만 아니라 회전속도(angular)도 완벽하게 0으로 잠급니다.
            rb.linearVelocity = Vector2.zero; 
            rb.angularVelocity = 0f; 
        }

#endregion


        #region 물고기 방향 및 스프라이트 뒤집기

        /// <summary>
        /// 왕복 구간(편도)이 하나 끝날 때마다 DOTween이 호출하는 콜백.
        /// 다음 목표가 어느 쪽인지 토글하고, 그에 맞춰 스프라이트 방향(LocalScale x값)을 갱신한다.
        
        /// </summary>
        private void HandleStepComplete()
        {   
            
            // 현재 가고 있는 대상 지점 (patrolPosition 또는 spawnPosition 과의 X축 차이를 구한다
            goingToPatrol = !goingToPatrol;
            float directionX = goingToPatrol
                ? patrolPosition.x - rb.position.x
                : spawnPosition.x - patrolPosition.x;
            
            ApplyingFacing(directionX);
        }
        
        /// <summary>
        /// 이동 방향(x축 기준)에 맞춰 물고기의 좌우 스프라이트 방향을 반전시킨다.
        /// localScale.x의 부호만 바꾸므로 자식 오브젝트(bone_1, bone_2 등)에도
        /// 함께 적용된다. 원본 프리팹은 왼쪽을 바라보는 상태를 기준으로 한다.
        /// </summary>
        /// <param name="directionX">
        /// 다음 목표 지점까지의 x축 방향값. 양수면 오른쪽 이동, 음수면 왼쪽 이동을 의미한다.
        /// </param>
        private void ApplyingFacing(float directionX)
        {
            
            if (Mathf.Abs(directionX) < 0.001f) return; //  거의 수직 이동이라면 무시
            
            Vector3 scale = transform.localScale;
            // 원본이 왼쪽을 바라보고 있어서 오른쪽 이동 (directionX > 0 ) 시 반전
            
            scale.x = directionX > 0f ? -baseScaleX : baseScaleX;
            transform.localScale = scale;
            
        }
        

        #endregion
        
        #region Caught 상태에서의 발버둥 (ApplyStruggleForce)

        public void InitStruggle()
        {
            directionTimer = 0f;
            // 최초 발버둥 방향 역시 공용 도망 방향 로직을 그대로 사용합니다.
            struggleDirection = GetFleeDirection();
        }

        /// <summary>
        /// 잡혀있는 동안 공용 도망 방향 알고리즘을 사용해 주기적으로 방향을 꺾으며 발버둥칩니다.
        /// </summary>
        public void ApplyStruggleForce(float resistanceRatio)
        {
            directionTimer += Time.deltaTime;

            if (directionTimer >= directionChangeInterval)
            {
                directionTimer = 0f;
                // [재활용]: 기존에 설계된 검증된 도망 방향 함수를 그대로 호출!
                struggleDirection = GetFleeDirection();
            }

            float currentForce = maxStruggleForce * resistanceRatio;
            rb.AddForce(struggleDirection * currentForce, ForceMode2D.Force);

            // 물리 진행 방향에 따라 머리 방향 자연스럽게 회전
            if (struggleDirection != Vector2.zero && currentForce > 0.1f)
            {
                float targetAngle = Mathf.Atan2(struggleDirection.y, struggleDirection.x) * Mathf.Rad2Deg;
                rb.MoveRotation(Mathf.LerpAngle(rb.rotation, targetAngle - 90f, Time.deltaTime * 10f));
            }
        }

        #endregion
        
        
        
        
        /// <summary>
        /// Raycast 대신 BoxCast를 사용하여 부피가 있는 장애물 감지를 처리합니다.
        /// </summary>
        private void CheckForObstacles()
        {
            Vector3 targetPos = goingToPatrol ? patrolPosition : spawnPosition;
            Vector2 moveDirection = (targetPos - transform.position).normalized;

            // 2D BoxCast: 내 위치에서 지정한 크기의 상자(obstacleCheckSize)를 진행 방향으로 쏩니다.
            RaycastHit2D hit = Physics2D.BoxCast(
                transform.position,       // 시작점 (물고기 중심)
                obstacleCheckSize,        // 박스의 크기 (가로, 세로)
                0f,                       // 박스의 회전 각도
                moveDirection,            // 발사 방향
                obstacleCheckDistance,    // 발사 거리
                obstacleLayer             // 대상 레이어
            );

            if (hit.collider != null)
            {
                TurnAroundImmediately();
            }
        }
        
        #region 디버깅용 기즈모 드로잉 (BoxCast 시각화)

        private void OnDrawGizmosSelected()
        {
            if (Application.isPlaying)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawLine(spawnPosition, patrolPosition);
                Gizmos.DrawWireSphere(patrolPosition, 0.3f);
            }

            // BoxCast의 스캔 영역을 빨간색 상자로 시각화합니다.
            Gizmos.color = Color.red;
            Vector3 targetPos = goingToPatrol ? patrolPosition : spawnPosition;
            Vector3 dir = (targetPos - transform.position).normalized;

            // 1. 물고기 위치에 있는 감지 상자 시작점
            Gizmos.DrawWireCube(transform.position, new Vector3(obstacleCheckSize.x, obstacleCheckSize.y, 0f));
            
            // 2. 최대 감지 거리에 있는 감지 상자 끝점
            Vector3 endPosition = transform.position + dir * obstacleCheckDistance;
            Gizmos.DrawWireCube(endPosition, new Vector3(obstacleCheckSize.x, obstacleCheckSize.y, 0f));

            // 두 상자를 잇는 선
            Gizmos.DrawLine(transform.position, endPosition);
        }

        #endregion
    }
}



