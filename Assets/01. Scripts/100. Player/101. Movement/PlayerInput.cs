using Cysharp.Threading.Tasks;
using HelloServer;
using UnityEngine;

namespace WeGotOne
{
    public class PlayerInput : MonoBehaviour
    {
        [SerializeField] private PlayerInputContainer inputContainer;

        [SerializeField] private PlayerInputController inputController;

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
        
        public void ApplyPosition(Vector2 moveInput, Quaternion rotInput)
        {
            if (isHost)
                return;

            inputController.SetArmPosition(moveInput, rotInput);
        }
    }
}