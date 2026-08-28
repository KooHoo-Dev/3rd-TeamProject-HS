using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using WeGotOne;

namespace HelloServer
{
    // 내가 조종하는 캐릭터. 씬에 하나 놓고 RoomClient 에 연결한다.
    public class LocalPlayer : MonoBehaviour
    {
        [Header("모양")]
        [SerializeField] private SpriteRenderer body;
        [SerializeField] private TMP_Text label;

        [SerializeField] public PlayerInputContainer InputContainer;

        [Header("움직임")]
        [SerializeField] private float speed = 5f;

        public bool CanMove { get; set; } = true;
        
        [Header("입력값")]
        public Keyboard Keyboard { get; private set; }

        private void Awake()
        {
            InputContainer = GetComponent<PlayerInputContainer>();
        }

        public void Setup(User user, Color color)
        {
            label.text = $"{user.NickName} ({user.Id})";
            body.color = color;
        }

        private void Update()
        {
            if (CanMove == false) return;

            Keyboard = Keyboard.current;

            float x = (Keyboard.rightArrowKey.isPressed ? 1 : 0) - (Keyboard.leftArrowKey.isPressed ? 1 : 0);
            float y = (Keyboard.upArrowKey.isPressed ? 1 : 0) - (Keyboard.downArrowKey.isPressed ? 1 : 0);

            transform.Translate(new Vector3(x, y) * (speed * Time.deltaTime));
        }
    }
}
