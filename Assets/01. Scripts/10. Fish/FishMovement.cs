using System;
using DG.Tweening;
using UnityEngine;


namespace WeGotOne
{
    public class FishMovement : MonoBehaviour
    {
        
        // prefab 으로 ObjectPulling 할 경우 위치 값을 세팅해야 하기 때문에 
        // spawn 위치를 배치 된 값으로 정하자.

        public Transform spawnPoint; // 물고기의 시작(귀환) 지점, 오프젝트 풀링 사용시 별도 세팅 필요
        public Transform patrolPoint; // 물고기의 순찰 포인트, 인스펙터에서 씬에 배치된 오브젝트를 직접 드래그하여 할당
        
        [SerializeField]
        [Tooltip("물고기의 이동속도: .SetSpeedBased 적용으로 거리와 무관하게 일정 속도 유지")]
        private float moveSpeed = 3f; // 물고기의 이동 속도
        
        private float registance; // 저항도
        private Animator anim;
        private Rigidbody2D rb; 
        private Tween patrolTween;

        private float baseScaleX; // 물고기 스케일 절대 값
        private bool goingToPatrol; // 현재 patrolPoint 로 가는 중인지
        
        private void Awake()
        {   
            // 물고기의 생성 지점은 배치된 지점
            
            anim = GetComponent<Animator>();
            rb = GetComponent<Rigidbody2D>();
            baseScaleX = Mathf.Abs(transform.localScale.x);
        }
        
        
        /// <summary>
        /// spawnPoint <-> patrolPoint 사이를 무한 왕복하는 순찰이동  
        /// </summary>
        public void patrolMove()
        {   
            if (spawnPoint == null || patrolPoint == null) 
            {
                Debug.LogWarning($"{name} : SpawnPoint or Patrol point 가 미설정 되었음");
                return;
            }
            
            // 중복 호출 시 트윈 중복 방지
            patrolTween.Kill();

            goingToPatrol= true;
            ApplyingFacing(patrolPoint.position.x - spawnPoint.position.x);

            patrolTween = rb
                .DOMove(patrolPoint.position, moveSpeed)
                .SetSpeedBased() // moveSpeed 를 duration 이 아닌 속도로 해석
                .SetLoops(-1, LoopType.Yoyo) // 무한 반복 루프
                .SetEase(Ease.Linear)
                .OnStepComplete(HandleStepComplete); // 한 구간(편도) 완료 시 마다 방향전환 호출


        }

        /// <summary>
        /// 왕복 구간(편도)이 하나 끝날 때마다 DOTween이 호출하는 콜백.
        /// 다음 목표가 어느 쪽인지 토글하고, 그에 맞춰 스프라이트 방향(LocalScale x값)을 갱신한다.
        
        /// </summary>
        private void HandleStepComplete()
        {
            goingToPatrol = !goingToPatrol;
            float directionX = goingToPatrol
             ? patrolPoint.position.x - rb.position.x
             : spawnPoint.position.x - patrolPoint.position.x;
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
        
        
        
        
        
        


    }
}



