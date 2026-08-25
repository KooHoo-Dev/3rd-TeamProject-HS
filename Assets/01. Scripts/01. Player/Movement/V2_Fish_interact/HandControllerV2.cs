using System;
using System.Collections.Generic;
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
    public class HandControllerV2 : MonoBehaviour
    {
        // 잡은 물체 저장용
        private Collider2D currentTarget;
        private Rigidbody2D grabbedTarget;
        
        public bool IsGrabbing => isGrabbing;
        public Rigidbody2D GrabbedTarget => grabbedTarget;
        
        // 잡기에 대한 상태
        private bool isGrabbing;
        
        // 팔에 잡은 물체가 있는지 알려주는 용도
        public event Action<HandControllerV2, Rigidbody2D> Grabbed; 
        public event Action<HandControllerV2> Released; 
        
# region  유니티 함수
        
        // 트리거에 물체 잡히면 타겟을 물체로 지정
        private void OnTriggerEnter2D(Collider2D other)
        {
            currentTarget = other;
        }
        
        // 물체 나가면 풀어주기 (타겟만 풀어줌)
        private void OnTriggerExit2D(Collider2D other)
        {
            if(currentTarget == other) currentTarget = null;
        }

        private void OnEnable() => HandRegistry.Register(this);
        
        // [보완 사항]: 손 오브젝트가 비활성화되거나 파괴될 때, 정적(static) 목록에서 확실히 제거해 주어야 
        // 씬 전환이나 오브젝트 풀 반환 시 NullReferenceException과 같은 메모리 누수가 발생하지 않습니다.
        private void OnDisable() => HandRegistry.Unregister(this);

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
            Grabbed?.Invoke(this, grabbedTarget);
        }
        
        // 그냥 풀어주고 옵저버 알려주기
        public void Release()
        {
            if (!isGrabbing)
                return;
            
            grabbedTarget = null;
            isGrabbing = false;
            
            Released?.Invoke(this);
        }
# endregion
    }

    #region 물고기 잡기 판정용 static 클래스 (활성화 된 손이 무엇이 있는지 등록해줌)

    public static class HandRegistry
    {
        private static readonly List<HandControllerV2> activeHands = new();

        public static List<HandControllerV2> ActiveHands => activeHands;
        public static void Register(HandControllerV2 hand) => activeHands.Add(hand);
        public static void Unregister(HandControllerV2 hand) => activeHands.Remove(hand);
    }

    #endregion
}