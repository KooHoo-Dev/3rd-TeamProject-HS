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
        
        
        private Vector3 spawnPosition; // 물고기의 시작(귀환) 지점, 
        private Vector3 patrolPosition; // 물고기의 순찰 포인트, 
        private float registance; // 저항도
        private Animator anim;
        private Rigidbody2D rb; 
        private Tween patrolTween; // 순찰 트윈
        private Tween runawayTween;  // 도망 트윈


        private float baseScaleX; // 물고기 스케일 절대 값
        private bool goingToPatrol; // 현재 patrolPoint 로 가는 중인지
        private bool isEvading = false; // 장애물 회피 이동 중인지 여부
        
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
            
            isEvading = false; 
            
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
            isEvading = false;
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
        /// 장애물 감지 즉시 현재 경로를 포기하고 반대 방향으로 즉시 복귀시킵니다.
        /// </summary>
        private void TurnAroundImmediately()
        {
            Debug.LogWarning($"{name} : 전방에 장애물 [{obstacleLayer.value}] 감지! 즉시 회피 및 반대 방향으로 복귀합니다.");
            
            StopMove();
            isEvading = true;

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
            // 1. 좌우(X축) 랜덤 오프셋 결정
            float randomDirectionX = Random.value > 0.5f ? 1f : -1f;
            float offsetX = randomDirectionX * patrolRange; 

            // 2. 상하(Y축) 랜덤 오프셋 결정 (항상 좌우보다 좁게 움직이도록 제한)
            float verticalRange = patrolRange * Random.Range(0.2f, 0.5f);
            float randomDirectionY = Random.value > 0.5f ? 1f : -1f;
            float offsetY = randomDirectionY * verticalRange;

            // 3. 최종 목표 지점 설정
            this.patrolPosition = spawnPosition + new Vector3(offsetX, offsetY, 0f);
        }
        
        /// <summary>
        /// 플레이어를 비스듬히 피해 무작위 대각선 방향으로 빠르게 도망치는 비동기 메서드
        /// </summary>
        public async Task RunawayMoveAsync(CancellationToken token)
        {
            // 1. 기존에 진행 중인 모든 순찰 및 회피 트윈 안전하게 정지
            StopMove();
            runawayTween?.Kill();

            // 2. 도망갈 임의의 대각선 방향 벡터 계산 (X축은 양방향 랜덤, Y축은 위아래로 부드럽게 랜덤)
            float fleeDirX = UnityEngine.Random.value > 0.5f ? 1f : -1f;
            float fleeDirY = UnityEngine.Random.Range(-0.3f, 0.3f); // 너무 위아래로 수직 상승/하강 하지 않게 보정
            Vector3 fleeDirection = new Vector3(fleeDirX, fleeDirY, 0f).normalized;

            // 3. 현재 위치 기준 최종 도망 목표 좌표 설정
            Vector3 targetPosition = transform.position + fleeDirection * runawayDistance;
    
            // 도망치는 진행 방향에 맞춰 물고기 좌우 스프라이트 반전
            ApplyingFacing(fleeDirection.x);

            // 4. 순찰 속도보다 훨씬 빠른 속도로 순간 질주
            float speed = moveSpeed * runawaySpeedMultiplier;

            // 5. 순간적으로 확 치고 나갔다가 서서히 감속하는 OutQuad 이징 트윈 실행
            runawayTween = rb.DOMove(targetPosition, speed)
                .SetSpeedBased()
                .SetEase(Ease.OutQuad);

            try
            {
                // 6. 비동기로 도망 트윈이 완벽히 끝날 때까지 렌더링 프레임을 양보하며 대기
                while (runawayTween != null && runawayTween.IsActive() && runawayTween.IsPlaying() && !token.IsCancellationRequested)
                {
                    await Task.Yield(); // 매 프레임 대기하며 트윈 플레이 확인 (GC 최소화 및 호환성 극대화)
                }
            }
            finally
            {
                // 예외 상황이나 취소 토큰 작동 시(예: 도망 도중 다른 상태로 전이됨) 안전하게 트윈 정리
                if (token.IsCancellationRequested)
                {
                    runawayTween?.Kill();
                    runawayTween = null;
                }
            }
        }
        
        
        
        public void StopMove()
        {
            if (patrolTween != null)
            {
                patrolTween.Kill();
                patrolTween = null;
            }
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



