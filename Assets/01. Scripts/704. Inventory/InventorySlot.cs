using TMPro;
using UnityEngine;
using UnityEngine.UI;


namespace WeGotOne
{
    public class InventorySlot : MonoBehaviour
    {
        // Slot은 전달받은 아이템과 수량만 표시합니다.
        // Cursor Slot은 위치 설정 기능도 사용합니다.

        [field: SerializeField] public Image Content { get; private set; }
        [field: SerializeField] public TMP_Text ItemCountText { get; private set; }

        private RectTransform rectTransform;

        private void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
            Content = GetComponentInChildren<Image>();
            ItemCountText = GetComponentInChildren<TMP_Text>();
        }

        public void Render(ItemScriptableObject item, int count)
        {
            bool shouldDisplay = item != null && count > 0;

            Content.sprite = shouldDisplay ? item.Icon : null;
            Content.enabled = shouldDisplay;
            ItemCountText.text = shouldDisplay ? count.ToString() : string.Empty;
            ItemCountText.enabled = shouldDisplay;
        }

        // 외부에서 위치값을 입력받아서 Slot의 위치를 갱신합니다.
        // Cursor 슬롯 전용 함수입니다.
        public void SetPosition(Vector2 inputPosition)
        {
            rectTransform.anchoredPosition = inputPosition;
        }
    }
}
