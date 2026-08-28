using Cysharp.Threading.Tasks;
using HelloServer;
using UnityEngine;

namespace WeGotOne
{
    public class PlayerInput : MonoBehaviour
    {
        [SerializeField] private PlayerInputContainer inputContainer;

        [SerializeField] private PlayerInputController inputController;
        public PlayerInputController InputController => inputController;

        private bool isHost;
        private bool isInitialized;

        public void Setup(bool host)
        {
            isHost = host;
            isInitialized = true;
        }

        private void Update()
        {
            if (isInitialized == false)
                return;

            Vector2 moveInput = inputContainer.MoveInput;
            bool leftGrab = inputContainer.LeftGrabInput;
            bool rightGrab = inputContainer.RightGrabInput;

            if (isHost)
            {
                inputController.ApplyInput(moveInput, leftGrab, rightGrab);
            }
        }

        public void ApplyNetworkInput(Vector2 moveInput, bool leftGrab, bool rightGrab)
        {
            if (isHost)
                return;

            inputController.ApplyInput(moveInput, leftGrab, rightGrab);
        }
        
        public void ApplyPosition(PlayerState state)
        {
            if (isHost)
                return;

            inputController.SetBodyPartPosition(state.Body.X,  state.Body.Y, state.Body.QuaternionZ);
            inputController.SetLeftArmTopPartPosition(state.LeftArmTop.X,  state.LeftArmTop.Y, state.LeftArmTop.QuaternionZ);
            inputController.SetLeftArmBottomPartPosition(state.LeftArmBottom.X,  state.LeftArmBottom.Y, state.LeftArmBottom.QuaternionZ);
            inputController.SetRightArmTopPartPosition(state.RightArmTop.X,  state.RightArmTop.Y, state.RightArmTop.QuaternionZ);
            inputController.SetRightArmBottomPartPosition(state.RightArmBottom.X,  state.RightArmBottom.Y, state.RightArmBottom.QuaternionZ);
        }
    }
}