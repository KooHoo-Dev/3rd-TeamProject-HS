using UnityEngine;
using UnityEngine.InputSystem;

namespace WeGotOne
{
    public class PlayerInputContainer : MonoBehaviour
    {
        public Vector2 MoveInput { get; private set; }
        public bool LeftGrabInput { get; private set; }
        public bool RightGrabInput { get; private set; }

        private void Update()
        {
            MoveInput = SimpleInput.GetMoveAxisRaw();

            LeftGrabInput = SimpleInput.GetKey(Key.LeftShift);
            RightGrabInput = SimpleInput.GetKey(Key.RightShift);
        }
    }
}