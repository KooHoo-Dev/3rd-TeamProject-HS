using UnityEngine;

namespace WeGotOne
{
    /*
     * 최종 구조 : 해당 클래스는 팔의 움직임을 담당하며,
     *            -- 인스펙터 부분 --
     *            기본적으로 'RigidBody 2D, Collider 2D, Hinge Joint 2D'를 가진 것을 전제로 함
     *            이때 Hinge Joint 2D에 연결된 Rigid Body는 Body임
     *            Hinge Joint 2D의 제한값을 적용하고 제한값을 몸통의 바깥 방향 부분으로만 각이 벌어지게 해놨음
     *
     *            -- 구현 부분 --
     *            팔은 '기본 힘'을 가지고 물체가 가지고 있는 중력값, 질량에 따라서 추가 힘을 부여받음
     *            단 추가힘 또한 최대치를 정하여 무거운 물건조차 혼자 들 수 있는 상황을 방지하고, 협동이 무너지는 일을 막았음
     *
     *            플레이어에 입력값에 따라서 방향을 정하고, 해당 방향으로 기본 힘의 크기로 이동하는 것이 일반적인 움직임
     */
    
    [RequireComponent(typeof(Rigidbody2D), typeof(Collider2D), typeof(HingeJoint2D))]
    public class ArmController : MonoBehaviour
    {
        public Vector2 InputDirection => inputDirection;
        
        // 기본적으로 사용할 힘
        [SerializeField] private float defaultForce;
        
        // 잡은 물체에 따른 추가적인 힘과 제한값
        [SerializeField] private float forceMultiplierOfMass;
        [SerializeField] private float forceLimit;
        
        // 런타임용. 없앨 가능성 높음
        [SerializeField] public float currentForce;
        
        // [수정]: 기존 HandController에서 인터페이스가 적용된 새로운 클래스명으로 변경
        [SerializeField] private HandControllerV2 hand;
        protected HandControllerV2 Hand => hand;
        
        private Rigidbody2D rigid;
        private Vector2 inputDirection;

        // Hand에서 받아와서 타겟이랑 잡은 상태인지 판단하기. 추가힘 쓸때 사용함
        [SerializeField] private Rigidbody2D grabbedTarget;
        [SerializeField] private bool isGrabbing;
        
        private void Awake()
        {
            rigid = GetComponent<Rigidbody2D>();
        }
        
#region Hand 이벤트 구독 함수 (타겟 넣어주고 빼주고 한거)

        private void OnEnable()
        {
            hand.Grabbed += OnGrabbed;
            hand.Released += OnReleased;
        }

        private void OnDisable()
        {
            // [보완]: 안전한 언구독을 위해 Null 조건부 연산자(?.)를 사용하면 좋습니다.
            if (hand != null)
            {
                hand.Grabbed -= OnGrabbed;
                hand.Released -= OnReleased;
            }
        }
        
        // [수정]: 새로운 델리게이트 시그니처에 맞춰 첫 번째 매개변수로 이벤트를 보낸 손(hand)을 받도록 가상 함수 수정
        protected virtual void OnGrabbed(HandControllerV2 hand, Rigidbody2D target)
        {
            grabbedTarget = target;
            isGrabbing = true;
        }

        // [수정]: 이벤트를 보낸 손(hand)을 받도록 가상 함수 수정
        protected virtual void OnReleased(HandControllerV2 hand)
        {
            grabbedTarget = null;
            isGrabbing = false;
        }

#endregion

#region 플레이어 입력값 처리 (PlayerInputController에서 사용)

        public void SetDirection(Vector2 direction)
        {
            inputDirection = direction.normalized;
        }

        public void ClearDirection()
        {
            inputDirection = Vector2.zero;
        }

#endregion

        private void FixedUpdate()
        {
            MoveToDirection(inputDirection);
        }

        private void MoveToDirection(Vector2 direction)
        {
            if (inputDirection == Vector2.zero)
                return;

            // float currentForce = CalculateForce(); 런타임용으로 밖으로 빼놨음. 빌드할땐 이거 살리고 밑에 죽일꺼
            currentForce = CalculateForce();
            
            rigid.AddForce(inputDirection * currentForce);

            // 임시 코드. 공중같이 중력 영향 외 물리력 안받는 상황에서
            // 팔로 무한 가속 되는거 막는 용도
            if (rigid.linearVelocity.magnitude > 5f)
            {
                rigid.AddForce(-inputDirection * currentForce);
            }
        }

        /*
         * 물체의 무게에 따른 추가 힘
         * 기본적으로 정적이거나, 정적인 취급을 받는 물체 (나무같이 무거운거)는 최대힘 처리
         * (지금 임시로 나무도 최대힘 처린데 최대값 늘리고 개별 힘만 좀 조절해주면 나무도 개별 처리 되긴 할꺼)
         * (근데 협동할꺼면 최대힘 처리가 맞는듯)
         * 그외에는 무게 계산해서 추가적으로만 부여
         */
        private float CalculateForce()
        {
            // 기본힘 넣어놓고 시작
            float result = defaultForce;
            
            // 잡은거 없으면 기본 힘
            if (isGrabbing == false)
                return result;
            
            // 물체가 정적이면 최대힘 처리
            if(grabbedTarget == null)
                return forceLimit;
            
            if(grabbedTarget.bodyType == RigidbodyType2D.Static ||
               grabbedTarget.bodyType == RigidbodyType2D.Kinematic)
                return forceLimit;

            // 물체 질량 * 배수로 추가힘 부여 (중력이 9.8이라 대충 10으로 곱했음)
            float bonusForce = grabbedTarget.mass * forceMultiplierOfMass;
            
            result += bonusForce;
            
            // 최대힘 못넘어가게 처리
            return Mathf.Min(result, forceLimit);
        }
        
        # region 치트 모드

        public void SetForce(float force)
        {
            defaultForce = force;
        }

        # endregion
    }
}