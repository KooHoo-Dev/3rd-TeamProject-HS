using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WeGotOne.Fish_Interface;

namespace WeGotOne
{
    /*
     * 손의 상태를 담당하는 클래스 (V2)
     * 구조 : 물리적인 움직임 연산 대신 Collider 2D 트리거를 통해 물체 접촉을 감지하며,
     *       접촉 시 일정 시간 내에 잡지 않으면 대상에게 OnMissed 신호를 보냅니다.
     *       잡거나 놓았을 때의 모든 신호 주도권을 가집니다.
     */
    [RequireComponent(typeof(Collider2D))]
    public class HandControllerV2 : MonoBehaviour
    {
        private int waterLayer = LayerMask.NameToLayer("Water");
        
        // 잡은 물체 저장용
        private Collider2D currentTarget;
        private Rigidbody2D grabbedTarget;
        
        // 원래 enter2D에서 물체 하나씩만 저장했는데 단체로 저장하고 0번만 빼오게 해야될듯
        private List<Collider2D> targets;
        
        public bool IsGrabbing => isGrabbing;
        public Rigidbody2D GrabbedTarget => grabbedTarget;
        
        // 잡기에 대한 상태
        private bool isGrabbing;
        
        // 팔(ArmController)이 무게 계산용 추가 힘을 적용할 수 있도록 등록하는 이벤트
        public event Action<Rigidbody2D> Grabbed; 
        public event Action Released; 
        
        // 잡았을때 변경해줄 스프라이트. 애니메이션 아니니까 스프라이트 갈아끼울게요.
        [SerializeField] private Sprite grabSprite;
        [SerializeField] private Sprite handSprite;
        [SerializeField] private SpriteRenderer spriteRenderer;
        
        // 본인 신체 못잡게 하기 위한 용도
        private PlayerRoot owner;

        [Header("잡기 타이머 설정")]
        [SerializeField] 
        [Tooltip("물고기 등 잡을 수 있는 물체에 닿은 후 도망치기 전까지 주어지는 유예 시간")]
        private float grabWindowTime = 1.2f; 
        
        private Coroutine grabTimerCoroutine;

#region 유니티 생명주기 함수

        private void Awake()
        {
            // 신체 가장 최상단에 PlayerRoot를 달아서
            // 해당 컴포넌트가 동일하면 동일 신체로 보고 잡지 않을 예정
            // 신체 부위 추가되도 동일하게 본인 신체는 안잡힘
            owner = GetComponentInParent<PlayerRoot>();
            
            spriteRenderer = GetComponent<SpriteRenderer>();
            
            targets = new List<Collider2D>();
        }

private void OnDisable()
        {
            // 비활성화 시 작동 중인 타이머 안전하게 정리
            StopGrabTimer();
        }

        // 트리거에 물체가 들어왔을 때 처리
        private void OnTriggerEnter2D(Collider2D other)
        {
            PlayerRoot targetOwner = other.GetComponentInParent<PlayerRoot>();
            // 자기 자신의 신체라면 잡기 대상에서 제외
            if (targetOwner == owner)
                return;

            if (other.gameObject.layer == waterLayer)
                return;
            
            targets.Add(other);
            currentTarget = targets[0];

            // 닿은 물체가 낚을 수 있는 대상(ICatchable)인 경우, 손이 스스로 타이머를 작동시킵니다.
            if (other.TryGetComponent<ICatchable>(out var catchable))
            {
                StopGrabTimer();
                grabTimerCoroutine = StartCoroutine(CoGrabWindowTimer(catchable));
            }
        }
        
        // 물체 범위 밖으로 나갔을 때 처리
        private void OnTriggerExit2D(Collider2D other)
        {
            if (targets == null || targets.Count == 0)
                return;
            
            if (targets.Contains(other))
            {
                targets.Remove(other);
                
                // 목표 타겟 변경하거나 없애기
                if(targets.Count != 0)
                    currentTarget = targets[0];
                else
                    currentTarget = null;
                
                StopGrabTimer(); // 잡지 않고 손을 떼고 가버렸다면 도망 타이머 즉시 취소
            }
        }

#endregion

#region 잡기 핵심 코드
        
        public void Grab()
        {
            spriteRenderer.sprite = grabSprite;
            
            if (isGrabbing)
                return;

            if (currentTarget == null)
                return;

            // 잡기에 성공했으므로 더 이상 도망 타이머를 돌릴 필요가 없음
            StopGrabTimer();

            grabbedTarget = currentTarget.attachedRigidbody;
            isGrabbing = true;
            
            // 1. 상위 팔(ArmController)의 추가 힘 연산을 위한 이벤트 알림
            Grabbed?.Invoke(grabbedTarget);

            // 2. 잡은 대상(물고기, 도구 등)에게 직접 잡혔음을 실시간 통보
            if (currentTarget.TryGetComponent<ICatchable>(out var catchable))
            {
                catchable.OnGrabbed();
            }
        }
        
        public void Release()
        {
            spriteRenderer.sprite = handSprite;
            
            if (isGrabbing == false)
                return;

            // 1. 잡고 있던 대상에게 놓아주었음을 먼저 통보
            if (grabbedTarget != null && grabbedTarget.TryGetComponent<ICatchable>(out var catchable))
            {
                catchable.OnReleased();
            }
            
            grabbedTarget = null;
            isGrabbing = false;
            
            // 2. 상위 팔(ArmController)의 물리 힘 복원을 위한 이벤트 알림
            Released?.Invoke();
        }

#endregion

#region 타이머 코루틴 제어

        private IEnumerator CoGrabWindowTimer(ICatchable catchable)
        {
            yield return new WaitForSeconds(grabWindowTime);

            // 지정된 시간 동안 기다렸는데도 잡지(IsGrabbing) 못했다면, "놓쳤음"을 대상에게 통보
            if (!isGrabbing)
            {
                catchable.OnMissed();
            }
        }

        private void StopGrabTimer()
        {
            if (grabTimerCoroutine != null)
            {
                StopCoroutine(grabTimerCoroutine);
                grabTimerCoroutine = null;
            }
        }

#endregion
    }

}