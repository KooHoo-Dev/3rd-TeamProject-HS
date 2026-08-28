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
    
    public class PlayerInputController : MonoBehaviour
    {
        [Header("Arms")]
        [SerializeField] private ArmController leftUpperArm;
        [SerializeField] private ArmController rightUpperArm;
        [SerializeField] private BottomArmController leftBottomArm;
        [SerializeField] private BottomArmController rightBottomArm;
        [SerializeField] private PlayerHunger body;
        
        public ArmController  LeftUpperArm => leftUpperArm;
        public ArmController  RightUpperArm => rightUpperArm;
        public BottomArmController LeftBottomArm => leftBottomArm;
        public BottomArmController RightBottomArm => rightBottomArm;
        public PlayerHunger Body => body;

        public void ApplyInput(Vector2 moveInput, bool leftGrab, bool rightGrab)
        {
            HandleMovement(moveInput);
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

        #region 파츠별 위치 동기화

        public void SetBodyPartPosition(float x, float y, float rotationZ)
        {
            body.transform.position = new Vector2(x, y);
            body.transform.rotation = new Quaternion(body.transform.rotation.x, body.transform.rotation.y, rotationZ, body.transform.rotation.w);
        }
        
        public void SetLeftArmTopPartPosition(float x, float y, float rotationZ)
        {
            leftUpperArm.SetPosition(new Vector2(x, y), rotationZ);
        }
        
        public void SetLeftArmBottomPartPosition(float x, float y, float rotationZ)
        {
            leftBottomArm.SetPosition(new Vector2(x, y), rotationZ);
        }
        
        public void SetRightArmTopPartPosition(float x, float y, float rotationZ)
        {
            rightUpperArm.SetPosition(new Vector2(x, y), rotationZ);
        }
        
        public void SetRightArmBottomPartPosition(float x, float y, float rotationZ)
        {
            rightBottomArm.SetPosition(new Vector2(x, y), rotationZ);
        }

        #endregion
    }
}