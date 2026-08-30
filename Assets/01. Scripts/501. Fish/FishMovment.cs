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
        #region 인스펙터 설정 필드

        [Header("순찰(Patrol) 설정")]
        [SerializeField]
        [Tooltip("물고기의 이동속도: .SetSpeedBased 적용으로 거리와 무관하게 일정 속도 유지")]
        private float moveSpeed = 3f;

        [SerializeField]
        [Tooltip("물고기마다 개별적으로 가질 순찰 범위(좌우)")]
        private float patrolRange = 10f;

        [Header("도망(Runaway) 연출 설정")]
        [SerializeField]
        [Tooltip("도망갈 때의 속도 배율 (기존 순찰 속도 대비 몇 배 빠르게 달릴지)")]
        private float runawaySpeedMultiplier = 2.5f;

        [SerializeField]
        [Tooltip("도망치는 총 거리")]
        private float runawayDistance = 6.0f;

        [Header("힘겨루기(Struggle) 물리 설정")]
        [SerializeField] private float maxStruggleForce = 10f;         // 사방으로 발버둥치는 최대 물리 힘
        [SerializeField] private float directionChangeInterval = 0.3f;   // 도망치려는 방향을 바꾸는 주기 (초)

        [Header("물 밖 중력 설정")]
        [SerializeField] 
        [Tooltip("물 밖으로 나갔을 때 적용할 중력 수치")]
        private float airGravityScale = 2.0f;
        
        [Header("수중 복귀 설정")]
        [SerializeField]
        [Tooltip("물속에 들어온 뒤, 순찰(트윈)을 시작하기 전 대기할 지연 시간(초)")]
        private float patrolRestartDelay = 0.5f;

        
        

        #endregion

        #region 컴포넌트 및 트윈 캐싱 변수

        private Animator anim;
        private Rigidbody2D rb; 
        private FishObstacleSensor obstacleSensor; // 외부 장애물 센서 연결용

        private Tween patrolTween; 
        private Tween runawayTween;  
        private Tween driftTween; 

        #endregion

        #region 이동 제어 내부 상태 변수

        private Vector3 spawnPosition;  // 스폰 시작 위치
        private Vector3 patrolPosition; // 목적지 순찰 위치
        private Vector2 struggleDirection;
        
        private float directionTimer;
        private float baseScaleX;       // 물고기 스프라이트 원본 스케일 값
        
        private bool goingToPatrol;     // 현재 patrolPosition 방향으로 가는 중인지 여부
        

        #endregion

        #region 외부 정보 제공 프로퍼티 (센서 및 외부 클래스 연동용)

        // 장애물 센서가 물고기가 순찰 중일 때만 작동하도록 확인하는 판독 프로퍼티
        public bool IsPatrolling => patrolTween != null && patrolTween.IsActive() && patrolTween.IsPlaying();
        
        // 장애물 센서가 내 타겟 지점을 파악할 수 있도록 제공하는 프로퍼티
        public Vector3 TargetPosition => goingToPatrol ? patrolPosition : spawnPosition;

        #endregion

        #region 유니티 생명주기 (Awake & OnDestroy)

        private void Awake()
        {   
            anim = GetComponent<Animator>();
            rb = GetComponent<Rigidbody2D>();
            baseScaleX = Mathf.Abs(transform.localScale.x);

            // 1. 센서 컴포넌트를 가져옵니다.
            obstacleSensor = GetComponent<FishObstacleSensor>();
            
            // 2. 센서가 "앞에 장애물 있다!"고 알릴 때, 즉시 돌아가는 함수를 실행하도록 연결합니다.
            if (obstacleSensor != null)
            {
                obstacleSensor.OnObstacleDetected += TurnAroundImmediately;
            }
        }

        private void OnDestroy()
        {
            // 메모리 누수 방지를 위한 센서 이벤트 구독 해제
            if (obstacleSensor != null)
            {
                obstacleSensor.OnObstacleDetected -= TurnAroundImmediately;
            }
        }

        #endregion

        #region 풀링 시스템 초기화 및 기본 순찰

        /// <summary>
        /// 오브젝트 풀에서 꺼낼 때 호출할 초기화 함수
        /// </summary>
        public void PullingInit(Vector3 startPos)
        {   
            this.spawnPosition = startPos;
            transform.position = startPos;
            
            CalculateNewPatrolRoute();
            StartPatrol();
        }

        /// <summary>
        /// 왕복 순찰 트윈을 시작합니다.
        /// </summary>
        public void StartPatrol()
        {
            StopMove();
            goingToPatrol = true;
            
            ApplyingFacing(patrolPosition.x - spawnPosition.x);

            // [출발지 -> 목적지 -> 출발지] 1회 왕복 후 경로를 재설정하여 계속 순찰합니다.
            patrolTween = rb
                .DOMove(patrolPosition, moveSpeed)
                .SetSpeedBased() 
                .SetLoops(2, LoopType.Yoyo) 
                .SetEase(Ease.Linear)
                .OnStepComplete(HandleStepComplete) 
                .OnComplete(() =>
                {
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

        #endregion

        #region 물 출입 시 물리 제어 함수 (외력 / 중력)

        /// <summary>
        /// 물 밖으로 나갔을 때의 공중 물리 상태를 적용합니다.
        /// </summary>
        public void SetAirPhysics()
        {
            StopMove();                        // 트윈 이동 정지 (공중 수영 방지)
            rb.gravityScale = airGravityScale; // 설정해 둔 중력값 부여
        }

        /// <summary>
        /// 물 속으로 복귀했을 때 수중 물리 상태를 복구합니다.
        /// </summary>
        public void SetWaterPhysics()
        {
            rb.gravityScale = 0f;             // 중력 제거 (부력 상태)
            
            // 1. [보완] 뚝 멈추는 대신, 기존에 만들어 둔 'StartFaintDrift'를 사용해 
            // 물의 저항을 받듯 떨어지던 속도를 부드럽게 감속시킵니다.
            
            CalculateNewPatrolRoute(); // 순찰 방향 초기화
            StartFaintDrift(patrolRestartDelay);
            
    
            // 2. [보완] 예약되어 있던 순찰 실행이 있다면 꼬이지 않게 취소하고, 지연 대기 후 순찰을 시작합니다.
            CancelInvoke(nameof(StartPatrol));
            Invoke(nameof(StartPatrol), patrolRestartDelay);
        }

        #endregion

        #region 도망(Runaway) 및 관성 감속(Faint Drift)

        /// <summary>
        /// 플레이어를 비스듬히 피해 무작위 대각선 방향으로 빠르게 도망치는 비동기 메서드
        /// </summary>
        public async Task RunawayMoveAsync(CancellationToken token)
        {
            StopMove();
            runawayTween?.Kill();

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
        public void StartFaintDrift(float duration)
        {
            patrolTween?.Kill();
            runawayTween?.Kill();
            driftTween?.Kill();

            driftTween = DOTween.To(
                () => rb.linearVelocity, 
                x => rb.linearVelocity = x, 
                Vector2.zero, 
                duration
            ).SetEase(Ease.OutCubic);

            DOTween.To(
                () => rb.angularVelocity,
                x => rb.angularVelocity = x,
                0f,
                duration
            ).SetEase(Ease.OutCubic);
        }

        /// <summary>
        /// 실행 중인 모든 트윈을 멈추고 리지드바디 속도를 초기화하여 물고기를 멈춰 세웁니다.
        /// </summary>
        public void StopMove()
        {   
            // [보완] 대기 중이던 순찰 시작 예약도 안전하게 취소합니다.
            CancelInvoke(nameof(StartPatrol));
            
            patrolTween?.Kill();   patrolTween = null;
            runawayTween?.Kill();  runawayTween = null;
            driftTween?.Kill();    driftTween = null;
            
            rb.linearVelocity = Vector2.zero; 
            rb.angularVelocity = 0f; 
        }

        /// <summary>
        /// 대각선 도망/발버둥 칠 방향 벡터를 자연스럽게 제한하여 반환합니다.
        /// </summary>
        public Vector2 GetFleeDirection()
        {
            float fleeDirX = Random.value > 0.5f ? 1f : -1f;
            float fleeDirY = Random.Range(-0.3f, 0.3f); 
            return new Vector2(fleeDirX, fleeDirY).normalized;
        }

        #endregion

        #region 장애물 감지 대응 (회피 방향 전환)

        /// <summary>
        /// 장애물 감지 시 현재 경로를 포기하고 반대 방향으로 즉시 복귀시킵니다.
        /// </summary>
        private void TurnAroundImmediately()
        {
            
            StopMove();
            goingToPatrol = !goingToPatrol;

            Vector3 escapeTarget = goingToPatrol ? patrolPosition : spawnPosition;
            ApplyingFacing(escapeTarget.x - transform.position.x);

            patrolTween = rb.DOMove(escapeTarget, moveSpeed)
                .SetSpeedBased()
                .SetEase(Ease.Linear)
                .OnComplete(() =>
                {
                    CalculateNewPatrolRoute();
                    StartPatrol();
                });
        }

        #endregion

        #region 물고기 스프라이트 좌우 뒤집기 처리

        private void HandleStepComplete()
        {   
            goingToPatrol = !goingToPatrol;
            float directionX = goingToPatrol
                ? patrolPosition.x - rb.position.x
                : spawnPosition.x - patrolPosition.x;
            
            ApplyingFacing(directionX);
        }
        
        private void ApplyingFacing(float directionX)
        {
            if (Mathf.Abs(directionX) < 0.001f) return;
            
            Vector3 scale = transform.localScale;
            scale.x = directionX > 0f ? -baseScaleX : baseScaleX;
            transform.localScale = scale;
        }

        #endregion

        #region Caught 상태에서의 발버둥 물리처리

        public void InitStruggle()
        {
            directionTimer = 0f;
            struggleDirection = GetFleeDirection();
        }

        public void ApplyStruggleForce(float resistanceRatio)
        {
            directionTimer += Time.deltaTime;

            if (directionTimer >= directionChangeInterval)
            {
                directionTimer = 0f;
                struggleDirection = GetFleeDirection();
            }

            float currentForce = maxStruggleForce * resistanceRatio;
            rb.AddForce(struggleDirection * currentForce, ForceMode2D.Force);

            if (struggleDirection != Vector2.zero && currentForce > 0.1f)
            {
                float targetAngle = Mathf.Atan2(struggleDirection.y, struggleDirection.x) * Mathf.Rad2Deg;
                rb.MoveRotation(Mathf.LerpAngle(rb.rotation, targetAngle - 90f, Time.deltaTime * 10f));
            }
        }

        #endregion
    }
}