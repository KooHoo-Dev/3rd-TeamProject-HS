using DG.Tweening;
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


        public void Render(ItemRef item, int count)
        {
            bool shouldDisplay = item != null && count > 0;

            Content.sprite = shouldDisplay ? item.Icon : null;
            Content.enabled = shouldDisplay;
            ItemCountText.text = shouldDisplay ? count.ToString() : string.Empty;
            Content.transform.DOKill();
            Content.transform.localScale = Vector3.one;
            Content.transform.DOPunchScale(Vector3.one * 0.5f, 0.3f);
            ItemCountText.enabled = shouldDisplay;
        }
    }
}
