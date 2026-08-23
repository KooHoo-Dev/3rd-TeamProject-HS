using UnityEngine;

namespace WeGotOne
{
    /*
     * 팔 움직임에 관한 클래스
     * 예상 구조 : 상완은 AddTorque로 토크힘으로 회전시켜서 각을 만들고, 실제 힘을 받는건 하완이 하는 구조
     * 구현된 구조 : AddTorque로 하니까 회전축을 두개를 구현해야하는 문제가 생김
     *             (몸체에서 손 방향으로 힘을 줄때의 토크와 반대 상황에서의의 토크가 다름)
     *             해당 문제를 예상하지 못했어서 토크를 사용할 생각이었는데 해당 문제 겪고 나니
     *             힘 기반으로 작성하는게 더 편하기도 하고 깔끔해보여서 AddForce 사용했음
     *             그러고나니까 예상 구조에서 실제 상완, 하완의 차이점이 잡는 상태의 여부(한쪽 축을 동적으로 고정할지)라서
     *             하완만 따로 해당 클래스 상속 받아서 쓰기로 함
     *
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
        // 기본적으로 사용할 힘
        [SerializeField] private float defaultForce = 20f;
        
        // 잡은 물체에 따른 추가적인 힘과 제한값
        [SerializeField] private float forceMultiplierOfMass = 10f;
        [SerializeField] private float forceLimit = 180f;
        
        // 이벤트 구독용
        [SerializeField] private HandController hand;
        protected HandController Hand => hand;
        
        private Rigidbody2D rigid;
        private Vector2 inputDirection;

        // Hand에서 받아와서 타겟이랑 잡은 상태인지 판단하기. 추가힘 쓸때 사용함
        private Rigidbody2D grabbedTarget;
        private bool isGrabbing;
        
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
            hand.Grabbed -= OnGrabbed;
            hand.Released -= OnReleased;
        }
        
        protected virtual void OnGrabbed(Rigidbody2D target)
        {
            grabbedTarget = target;
            isGrabbing = true;
        }

        protected virtual void OnReleased()
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

            float currentForce = CalculateForce();

            rigid.AddForce(inputDirection * currentForce);
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
            if (!isGrabbing)
                return result;
            
            // 물체가 정적이면 최대힘 처리
            if(grabbedTarget == null)
                return forceLimit;

            // 물체 질량 * 배수로 추가힘 부여 (중력이 9.8이라 대충 10으로 곱했음)
            float bonusForce = grabbedTarget.mass * forceMultiplierOfMass;
            
            result += bonusForce;
            
            // 최대힘 못넘어가게 처리
            return Mathf.Min(result, forceLimit);
        }
    }
}