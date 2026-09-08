using HelloServer;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace WeGotOne
{
    public class GameStartOption : MonoBehaviour
    {
        [Header("처음 값")]
        [SerializeField] private string nickName = "꼬마돌";
        [SerializeField] private string roomCode = "1234";
        
        [SerializeField] private Button joinButton;
        [SerializeField] private TMP_Text joinButtonText;
        
        [SerializeField] private TMP_InputField nameText;
        [SerializeField] private TMP_Text namePreText;
        
        [SerializeField] private TMP_InputField roomText;
        
        // 컬러 피커 부분
        public Color selectedColor = Color.white;
        public Image circlePalette;
        public Image picker;
        public SpriteRenderer[] sprites;
        private CircleCollider2D paletteCollider;
        
        private void Awake()
        {
            paletteCollider = circlePalette.GetComponent<CircleCollider2D>();
        }
        
        private void Update()
        {
            bool canJoin = !RoomClient.Instance.IsBusy;

            joinButton.interactable = canJoin;

            joinButtonText.text = canJoin ? "들어가기" : "들어가는 중…";
        }
        
        public void JoinRoom()
        {
            if (RoomClient.Instance.IsBusy)
                return;
            
            RoomClient.Instance.Join(roomCode, nickName, selectedColor);
        }

        public void SetName()
        {
            nickName = nameText.text;
            namePreText.text = nameText.text;
        }

        public void SetRoomCode()
        {
            roomCode = roomText.text;
        }

        # region 컬러 피커 함수
        
        public void mousePointerDown()
        {
            selectColor();
            SetColor();
        }

        public void mouseDrag()
        {
            selectColor();
            SetColor();
        }

        public void SetColor()
        {
            for (int i = 0; i < sprites.Length; i++)
            {
                sprites[i].color = selectedColor;
            }
        }

        private void selectColor()
        {
            Vector2 mouseWorldPos = Camera.main.ScreenToWorldPoint(SimpleInput.GetMousePosition());

            // CircleCollider2D 범위 제한
            Vector2 center = paletteCollider.transform.TransformPoint(
                paletteCollider.offset
            );

            float radius = paletteCollider.radius *
                           Mathf.Max(
                               paletteCollider.transform.lossyScale.x,
                               paletteCollider.transform.lossyScale.y
                           );

            Vector2 direction = mouseWorldPos - center;

            if (direction.magnitude > radius)
            {
                mouseWorldPos = center + direction.normalized * radius;
            }

            // 기존 코드 그대로
            picker.transform.position = mouseWorldPos;

            RectTransform paletteRect = circlePalette.rectTransform;

            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                paletteRect,
                mouseWorldPos,
                null,
                out Vector2 localPos
            );

            float u = Mathf.InverseLerp(
                paletteRect.rect.xMin,
                paletteRect.rect.xMax,
                localPos.x
            );

            float v = Mathf.InverseLerp(
                paletteRect.rect.yMin,
                paletteRect.rect.yMax,
                localPos.y
            );

            Texture2D texture = circlePalette.mainTexture as Texture2D;

            selectedColor = texture.GetPixelBilinear(u, v);
            selectedColor.a = 1f;
        }
        
        # endregion
    }
}