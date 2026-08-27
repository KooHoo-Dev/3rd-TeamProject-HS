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

        private void Update()
        {
            HandleMovement();
            HandleGrab();
        }

        private void HandleMovement()
        {
            if (body.CanMove == false)
            {
                ClearArmDirections();
                return; 
            }
            
            Vector2 moveInput = SimpleInput.GetMoveAxisRaw();

            if (moveInput == Vector2.zero)
            {
                ClearArmDirections();
                return;
            }

            SetArmDirections(moveInput);
        }

        private void HandleGrab()
        {
            if (SimpleInput.GetKey(Key.LeftShift))
            {
                leftBottomArm.Grab();
            }

            if (SimpleInput.GetKeyUp(Key.LeftShift))
            {
                leftBottomArm.Release();
            }

            if (SimpleInput.GetKey(Key.RightShift))
            {
                rightBottomArm.Grab();
            }

            if (SimpleInput.GetKeyUp(Key.RightShift))
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
    }
}