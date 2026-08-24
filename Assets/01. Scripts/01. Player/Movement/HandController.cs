using System;
using UnityEngine;

namespace WeGotOne
{
    /*
     * 손의 상태를 담당하는 클래스
     * 구조 : 움직임등은 담당하지 않고 하완(BottomArm)에 자식으로써 위치값을 공유받는다.
     *       단순히 Collider 2D만을 컴포넌트로 가지고 트리거로써 물체와의 접촉이 있는지,
     *       접촉이 있을때 Grabbed 상태로 넘어가는지, 잡은 물체는 무엇인지 등을 담당한다.
     */
    
    [RequireComponent(typeof(Collider2D))]
    public class HandController : MonoBehaviour
    {
        // 잡은 물체 저장용
        private Collider2D currentTarget;
        private Rigidbody2D grabbedTarget;
        
        // 본인 신체 못잡게 하기 위한 용도
        private PlayerRoot owner;
        
        public bool IsGrabbing => isGrabbing;
        public Rigidbody2D GrabbedTarget => grabbedTarget;
        
        // 잡기에 대한 상태
        private bool isGrabbing;
        
        // 팔에 잡은 물체가 있는지 알려주는 용도
        public event Action<Rigidbody2D> Grabbed;
        public event Action Released;
        
# region  유니티 함수

        private void Awake()
        {
            // 신체 가장 최상단에 PlayerRoot를 달아서
            // 해당 컴포넌트가 동일하면 동일 신체로 보고 잡지 않을 예정
            // 신체 부위 추가되도 동일하게 본인 신체는 안잡힘
            owner = GetComponentInParent<PlayerRoot>();
        }

        // 트리거에 물체 잡히면 타겟을 물체로 지정
        private void OnTriggerEnter2D(Collider2D other)
        {
            PlayerRoot targetOwner = other.GetComponentInParent<PlayerRoot>();

            // 자기 자신의 신체라면 잡기 대상에서 제외
            if (targetOwner == owner)
                return;
            
            currentTarget = other;
        }

        // 물체 나가면 풀어주기 (타겟만 풀어줌)
        private void OnTriggerExit2D(Collider2D other)
        {
            if(currentTarget == other) currentTarget = null;
        }
        
#endregion

# region 잡기 코드

        /*
         *  잡기 상태에 관한 코드
         *  현재 잡은 상태거나,
         *  잡을 수 있는 물체가 없으면 (손에 물체가 안닿아있으면)
         *  리턴시킴
         *  그 후 타겟 저장하고 옵저버로 뿌림 (무게 계산해서 팔에 추가힘 부여하는건 팔이 함)
         */
        
        public void Grab()
        {
            if (isGrabbing)
                return;

            if (currentTarget == null)
                return;

            grabbedTarget = currentTarget.attachedRigidbody;
            isGrabbing = true;
            Grabbed?.Invoke(grabbedTarget);
        }
        
        // 그냥 풀어주고 옵저버 알려주기
        public void Release()
        {
            if (!isGrabbing)
                return;
            
            grabbedTarget = null;
            isGrabbing = false;
            
            Released?.Invoke();
        }
# endregion
    }
}