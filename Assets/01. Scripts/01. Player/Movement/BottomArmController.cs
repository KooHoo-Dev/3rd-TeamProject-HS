using UnityEngine;

namespace WeGotOne
{
    /*
     * ArmController의 기본 움직임을 사용함
     * 단 하완의 경우 추가적인 Hinge Joint 2D를 사용하여 잡기 시 Joint를 물체와 연결함
     */
    public class BottomArmController : ArmController
    {
        [SerializeField] private HingeJoint2D grabJoint;
        
        private void Start()
        {
            // 시작할때 잡기용 Joint 끄기
            grabJoint.enabled = false;
        }
        
        protected override void OnGrabbed(Rigidbody2D target)
        {
            // 잡기 상태 처리
            base.OnGrabbed(target);

            // 타겟에 리지드바디가 있을 경우
            if (target != null)
            {
                grabJoint.connectedBody = target;
            }
            // 타겟에 리지드 바디가 없을 경우. 정적 물체(벽, 바닥등) 처리
            else
            {
                grabJoint.connectedBody = null;
            }

            // 잡기 Joint 활성화
            grabJoint.enabled = true;
        }

        protected override void OnReleased()
        {
            // 잡기 Joint 해제
            grabJoint.enabled = false;
            grabJoint.connectedBody = null;

            // 상태 초기화
            base.OnReleased();
        }
        
        public void Grab()
        {
            Hand.Grab();
        }

        public void Release()
        {
            Hand.Release();
        }
    }
}
