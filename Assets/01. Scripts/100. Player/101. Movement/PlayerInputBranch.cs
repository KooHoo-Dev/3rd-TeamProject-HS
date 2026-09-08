using UnityEngine;
using UnityEngine.InputSystem;

namespace WeGotOne
{
    public class PlayerInputBranch : MonoBehaviour
    {
        /*
         * 실제 플레이어 입력을 분기해주는 클래스
         * 해당 클래스에서 서버로부터 SetUp을 받아온 다음
         * 호스트의 경우 : 직접 키보드의 값을 읽음
         */
        
        [SerializeField] private PlayerController controller;

        private void Update()
        {
            if (controller.User == null) return;
            
            // 호스트는 직접 키보드 입력으로 계산
            Vector2 moveInput = SimpleInput.GetMoveAxisRaw();
            bool leftGrab = SimpleInput.GetMouseButton(SimpleInput.MouseButton.Left);
            bool rightGrab = SimpleInput.GetMouseButton(SimpleInput.MouseButton.Right);
            bool buildKey = !IntegratedDebugUI.IsTeleportModeActive && SimpleInput.GetKey(Key.F);
                
            controller.ApplyInput(moveInput, leftGrab, rightGrab, buildKey);
        }
    }
}
