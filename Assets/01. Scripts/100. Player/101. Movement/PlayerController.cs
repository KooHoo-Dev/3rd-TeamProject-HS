using HelloServer;
using UnityEngine;

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
        [SerializeField] private PlayerStamina body;

        public bool IsLeftGrab { get; private set; }
        public bool IsRightGrab { get; private set; }
        public bool IsBuild { get; private set; }

        public User User { get; private set; }

        public ArmController LeftUpperArm => leftUpperArm;
        public ArmController RightUpperArm => rightUpperArm;
        public BottomArmController LeftBottomArm => leftBottomArm;
        public BottomArmController RightBottomArm => rightBottomArm;
        public PlayerStamina Body => body;

        #region 보간용

        // Snapshot 전송 주기
        private const float SnapshotInterval = 1f / 20f;

        private float interpolationTime;
        private bool initialized;

        private Vector2 previousBodyPosition;
        private Quaternion previousBodyRotation;

        private Vector2 previousLeftUpperPosition;
        private Quaternion previousLeftUpperRotation;

        private Vector2 previousRightUpperPosition;
        private Quaternion previousRightUpperRotation;

        private Vector2 previousLeftBottomPosition;
        private Quaternion previousLeftBottomRotation;

        private Vector2 previousRightBottomPosition;
        private Quaternion previousRightBottomRotation;

        private Vector2 targetBodyPosition;
        private Quaternion targetBodyRotation;

        private Vector2 targetLeftUpperPosition;
        private Quaternion targetLeftUpperRotation;

        private Vector2 targetRightUpperPosition;
        private Quaternion targetRightUpperRotation;

        private Vector2 targetLeftBottomPosition;
        private Quaternion targetLeftBottomRotation;

        private Vector2 targetRightBottomPosition;
        private Quaternion targetRightBottomRotation;

        #endregion

        private void Update()
        {
            if (initialized == false)
            {
                return;
            }

            interpolationTime += Time.deltaTime;

            float t = Mathf.Clamp01(interpolationTime / SnapshotInterval);
            ApplyInterpolatedTransform(body.transform, previousBodyPosition, targetBodyPosition, previousBodyRotation, targetBodyRotation, t);
            ApplyInterpolatedTransform(leftUpperArm.transform, previousLeftUpperPosition, targetLeftUpperPosition, previousLeftUpperRotation, targetLeftUpperRotation, t);
            ApplyInterpolatedTransform(rightUpperArm.transform, previousRightUpperPosition, targetRightUpperPosition, previousRightUpperRotation, targetRightUpperRotation, t);
            ApplyInterpolatedTransform(leftBottomArm.transform, previousLeftBottomPosition, targetLeftBottomPosition, previousLeftBottomRotation, targetLeftBottomRotation, t);
            ApplyInterpolatedTransform(rightBottomArm.transform, previousRightBottomPosition, targetRightBottomPosition, previousRightBottomRotation, targetRightBottomRotation, t);
        }

        public void Setup(User user)
        {
            User = user;
            
            // Body에 붙은 카메라 재설정
            SimpleFollowCamera cameraController = GetComponentInChildren<SimpleFollowCamera>(true);
            if (user.Id == RoomClient.Instance.Me.Id)
                cameraController.enabled = true;
        }
        
        public float GetHighestHungerForce()
        {
            float highestForce = 0f;

            if (leftUpperArm.CanConsumeAdditionalHunger)
            {
                highestForce = Mathf.Max(highestForce, leftUpperArm.CurrentForce);
            }
            if (rightUpperArm.CanConsumeAdditionalHunger)
            {
                highestForce = Mathf.Max(highestForce, rightUpperArm.CurrentForce);
            }
            if (leftBottomArm.CanConsumeAdditionalHunger)
            {
                highestForce = Mathf.Max(highestForce, leftBottomArm.CurrentForce);
            }
            if (rightBottomArm.CanConsumeAdditionalHunger)
            {
                highestForce = Mathf.Max(highestForce, rightBottomArm.CurrentForce);
            }

            return highestForce;
        }

        public void ApplyInput(Vector2 moveInput, bool leftGrab, bool rightGrab, bool isBuildKeyHold)
        {
            HandleMovement(moveInput);
            HandleGrab(leftGrab, rightGrab);
            IsBuild = isBuildKeyHold;
        }

        public void ApplySnapshot(PlayerState position, bool leftGrab, bool rightGrab)
        {
            body.ApplySnapshot(position.CurrentHunger);
            
            // 서버에서 들어온 값 저장
            Vector2 newBodyPosition = ToVector2(position.Body);
            Quaternion newBodyRotation = ToQuaternion(position.Body.Rotation);
            Vector2 newLeftUpperPosition = ToVector2(position.LeftArmTop);
            Quaternion newLeftUpperRotation = ToQuaternion(position.LeftArmTop.Rotation);
            Vector2 newRightUpperPosition = ToVector2(position.RightArmTop);
            Quaternion newRightUpperRotation = ToQuaternion(position.RightArmTop.Rotation);
            Vector2 newLeftBottomPosition = ToVector2(position.LeftArmBottom);
            Quaternion newLeftBottomRotation = ToQuaternion(position.LeftArmBottom.Rotation);
            Vector2 newRightBottomPosition = ToVector2(position.RightArmBottom);
            Quaternion newRightBottomRotation = ToQuaternion(position.RightArmBottom.Rotation);

            // 처음 Snapshot이면 즉시 적용
            if (initialized == false)
            {
                ApplyTransform(body.transform, newBodyPosition, newBodyRotation);
                ApplyTransform(leftUpperArm.transform, newLeftUpperPosition, newLeftUpperRotation);
                ApplyTransform(rightUpperArm.transform, newRightUpperPosition, newRightUpperRotation);
                ApplyTransform(leftBottomArm.transform, newLeftBottomPosition, newLeftBottomRotation);
                ApplyTransform(rightBottomArm.transform, newRightBottomPosition, newRightBottomRotation);

                SetSnapshotTargets(newBodyPosition, newBodyRotation, newLeftUpperPosition, newLeftUpperRotation, newRightUpperPosition, 
                    newRightUpperRotation, newLeftBottomPosition, newLeftBottomRotation, newRightBottomPosition, newRightBottomRotation);

                StorePreviousSnapshot();

                interpolationTime = SnapshotInterval;
                initialized = true;

                HandleGrab(leftGrab, rightGrab);
                return;
            }

            // 현재 target을 이전 Snapshot으로 이동
            StorePreviousSnapshot();

            // 새 Snapshot을 target으로 저장
            SetSnapshotTargets(newBodyPosition, newBodyRotation, newLeftUpperPosition, newLeftUpperRotation, newRightUpperPosition, 
                newRightUpperRotation, newLeftBottomPosition, newLeftBottomRotation, newRightBottomPosition, newRightBottomRotation);

            // 새로운 Snapshot 구간 보간 시작
            interpolationTime = 0f;

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

        #region Snapshot 보간 유틸

        private void StorePreviousSnapshot()
        {
            previousBodyPosition = targetBodyPosition;
            previousBodyRotation = targetBodyRotation;

            previousLeftUpperPosition = targetLeftUpperPosition;
            previousLeftUpperRotation = targetLeftUpperRotation;

            previousRightUpperPosition = targetRightUpperPosition;
            previousRightUpperRotation = targetRightUpperRotation;

            previousLeftBottomPosition = targetLeftBottomPosition;
            previousLeftBottomRotation = targetLeftBottomRotation;

            previousRightBottomPosition = targetRightBottomPosition;
            previousRightBottomRotation = targetRightBottomRotation;
        }

        private void SetSnapshotTargets(Vector2 bodyPosition, Quaternion bodyRotation, Vector2 leftUpperPosition, Quaternion leftUpperRotation, Vector2 rightUpperPosition,
            Quaternion rightUpperRotation, Vector2 leftBottomPosition, Quaternion leftBottomRotation, Vector2 rightBottomPosition, Quaternion rightBottomRotation)
        {
            targetBodyPosition = bodyPosition;
            targetBodyRotation = bodyRotation;

            targetLeftUpperPosition = leftUpperPosition;
            targetLeftUpperRotation = leftUpperRotation;

            targetRightUpperPosition = rightUpperPosition;
            targetRightUpperRotation = rightUpperRotation;

            targetLeftBottomPosition = leftBottomPosition;
            targetLeftBottomRotation = leftBottomRotation;

            targetRightBottomPosition = rightBottomPosition;
            targetRightBottomRotation = rightBottomRotation;
        }

        private void ApplyTransform(Transform target, Vector2 position, Quaternion rotation)
        {
            target.position = position;
            target.rotation = rotation;
        }

        private void ApplyInterpolatedTransform(Transform target, Vector2 previousPosition, Vector2 targetPosition, Quaternion previousRotation, Quaternion targetRotation, float t)
        {
            target.position = Vector2.Lerp(previousPosition, targetPosition, t);
            target.rotation = Quaternion.Lerp(previousRotation, targetRotation, t);
        }

        private Vector2 ToVector2(PartState partState)
        {
            return new Vector2(partState.X, partState.Y);
        }

        private Quaternion ToQuaternion(Rotation rotation)
        {
            return new Quaternion(rotation.X, rotation.Y, rotation.Z, rotation.W);
        }

        #endregion
    }
}
