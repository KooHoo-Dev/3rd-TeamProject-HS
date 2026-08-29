using HelloServer;
using UnityEngine;
using UnityEngine.InputSystem;

namespace WeGotOne
{
    /*
     * 플레이어 개체 :
     * 기본적으로 3가지로 분류되며, 총 7개의 오브젝트로 구성될 예정
     * Body : 직접적인 힘의 관여 없이 팔에 움직임에 따라서 흔들리거나 무게 중심을 잡아주는 역할
     *       단, 추후 하단으로 당겨주는 역할정도는 추가될 수 있음 (아랫방향으로 힘 지속적으로 줘서 무게중심 조금 더 잘 잡게)
     * 
     * Arm : 신체와 동일하게 상완, 하완으로 나누어서 상완은 신체와 겹치지 못하는 각을 만들고,
     *       하완이 자유롭게 회전 가능하게 만들어서 단순히 나뉘어졌다는 느낌보단
     *       상완은 각을 만들고 하완은 방향을 결정하는 느낌을 주려고 함
     *
     * Hand : 단순 축 고정용
     *        버튼 입력 들어오면 현재 손에 걸려있는 오브젝트가 있는지 확인 후 하완에게 전달.
     *        실제 고정은 하완의 끝을 고정시킴
     *
     * 해당 클래스는 실제 연산없이, 플레이어가 입력한 키값을 각각 부위에 맞게 뿌려주는 역할
     */
    
    public class PlayerController : MonoBehaviour
    {
        [Header("Arms")]
        [SerializeField] private ArmController leftUpperArm;
        [SerializeField] private ArmController rightUpperArm;
        [SerializeField] private BottomArmController leftBottomArm;
        [SerializeField] private BottomArmController rightBottomArm;
        [SerializeField] private PlayerHunger body;
        
        public bool IsLeftGrab { get; private set; }
        public bool IsRightGrab { get; private set; }

        public User User { get; private set; }
        
        public ArmController  LeftUpperArm => leftUpperArm;
        public ArmController  RightUpperArm => rightUpperArm;
        public BottomArmController LeftBottomArm => leftBottomArm;
        public BottomArmController RightBottomArm => rightBottomArm;
        public PlayerHunger Body => body;
        
        # region 보간용
        
        // 보간 주기. Snapshot 보내는 주기
        private const float SnapshotInterval = 1f / 20f;
        private float smooth = 10f;

        private float interpolationTime;
        private bool initialized;

        // private Vector2 previousBodyPosition;
        // private Quaternion previousBodyRotation;
        //
        // private Vector2 previousLeftUpperPosition;
        // private Quaternion previousLeftUpperRotation;
        //
        // private Vector2 previousRightUpperPosition;
        // private Quaternion previousRightUpperRotation;
        //
        // private Vector2 previousLeftBottomPosition;
        // private Quaternion previousLeftBottomRotation;
        //
        // private Vector2 previousRightBottomPosition;
        // private Quaternion previousRightBottomRotation;
        //
        // private Vector2 targetBodyPosition;
        // private Quaternion targetBodyRotation;
        //
        // private Vector2 targetLeftUpperPosition;
        // private Quaternion targetLeftUpperRotation;
        //
        // private Vector2 targetRightUpperPosition;
        // private Quaternion targetRightUpperRotation;
        //
        // private Vector2 targetLeftBottomPosition;
        // private Quaternion targetLeftBottomRotation;
        //
        // private Vector2 targetRightBottomPosition;
        // private Quaternion targetRightBottomRotation;
        
        # endregion
        
        private void Update()
        {
            if (initialized == false)
            {
                return;
            }

            interpolationTime += Time.deltaTime;
        }
        
        public void Setup(User user)
        {
            User = user;
        }
        
        public void ApplyInput(Vector2 moveInput, bool leftGrab, bool rightGrab)
        {
            HandleMovement(moveInput);
            HandleGrab(leftGrab, rightGrab);
        }
        
        public void ApplyPosition(PlayerState position, bool leftGrab, bool rightGrab)
        {
            // 서버에서 들어온 값 저장
            Vector2 newBodyPosition = new Vector2(position.Body.X, position.Body.Y);
            Quaternion newBodyRotation = ToQuaternion(position.Body.Rotation);
            Vector2 newLeftUpperPosition = new Vector2(position.LeftArmTop.X, position.LeftArmTop.Y);
            Quaternion newLeftUpperRotation = ToQuaternion(position.LeftArmTop.Rotation);
            Vector2 newRightUpperPosition = new Vector2(position.RightArmTop.X, position.RightArmTop.Y);
            Quaternion newRightUpperRotation = ToQuaternion(position.RightArmTop.Rotation);
            Vector2 newLeftBottomPosition = new Vector2(position.LeftArmBottom.X, position.LeftArmBottom.Y);
            Quaternion newLeftBottomRotation = ToQuaternion(position.LeftArmBottom.Rotation);
            Vector2 newRightBottomPosition = new Vector2(position.RightArmBottom.X, position.RightArmBottom.Y);
            Quaternion newRightBottomRotation = ToQuaternion(position.RightArmBottom.Rotation);

            // 값 저장하고 처음 적용이면 한번 적용
            if (initialized == false)
            {
                body.transform.position = newBodyPosition;
                body.transform.rotation = newBodyRotation;

                leftUpperArm.transform.position = newLeftUpperPosition;
                leftUpperArm.transform.rotation = newLeftUpperRotation;

                rightUpperArm.transform.position = newRightUpperPosition;
                rightUpperArm.transform.rotation = newRightUpperRotation;

                leftBottomArm.transform.position = newLeftBottomPosition;
                leftBottomArm.transform.rotation = newLeftBottomRotation;

                rightBottomArm.transform.position = newRightBottomPosition;
                rightBottomArm.transform.rotation = newRightBottomRotation;

                initialized = true;

                HandleGrab(leftGrab, rightGrab);
                return;
            }

            float t = Mathf.Clamp01(1f - Mathf.Exp(-smooth * Time.deltaTime));

            ApplyInterpolatedTransform(
                body.transform,
                newBodyPosition,
                newBodyRotation,
                t);

            ApplyInterpolatedTransform(
                leftUpperArm.transform,
                newLeftUpperPosition,
                newLeftUpperRotation,
                t);

            ApplyInterpolatedTransform(
                rightUpperArm.transform,
                newRightUpperPosition,
                newRightUpperRotation,
                t);

            ApplyInterpolatedTransform(
                leftBottomArm.transform,
                newLeftBottomPosition,
                newLeftBottomRotation,
                t);

            ApplyInterpolatedTransform(
                rightBottomArm.transform,
                newRightBottomPosition,
                newRightBottomRotation,
                t);

            HandleGrab(leftGrab, rightGrab);

            // interpolationTime = 0f;

            HandleGrab(leftGrab, rightGrab);
        }

        private void HandleMovement(Vector2 moveInput)
        {
            if (body.CanMove == false)
            {
                ClearArmDirections();
                return;
            }

            if (moveInput == Vector2.zero)
            {
                ClearArmDirections();
                return;
            }
            
            SetArmDirections(moveInput);
        }

        private void HandleGrab(bool leftGrab, bool rightGrab)
        {
            if (leftGrab)
            {
                leftBottomArm.Grab();
            }
            else
            {
                leftBottomArm.Release();
            }

            if (rightGrab)
            {
                rightBottomArm.Grab();
            }
            else
            {
                rightBottomArm.Release();
            }
            
            IsLeftGrab = leftGrab;
            IsRightGrab = rightGrab;
        }

        private void SetArmDirections(Vector2 direction)
        {
            leftUpperArm.SetDirection(direction);
            rightUpperArm.SetDirection(direction);
            leftBottomArm.SetDirection(direction);
            rightBottomArm.SetDirection(direction);
        }

        private void ClearArmDirections()
        {
            leftUpperArm.ClearDirection();
            rightUpperArm.ClearDirection();
            leftBottomArm.ClearDirection();
            rightBottomArm.ClearDirection();
        }

        private void ApplyInterpolatedTransform(
            Transform target,
            Vector2 newPosition,
            Quaternion newRotation,
            float t)
        {
            target.position = Vector2.Lerp(
                target.position,
                newPosition,
                t
            );

            target.rotation = Quaternion.Lerp(
                target.rotation,
                newRotation,
                t
            );
        } 

        private Quaternion ToQuaternion(Rotation rotation)
        {
            return new Quaternion(
                rotation.X,
                rotation.Y,
                rotation.Z,
                rotation.W
            );
        } 
    }
}