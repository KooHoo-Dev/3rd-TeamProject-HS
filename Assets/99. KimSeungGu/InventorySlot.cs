using UnityEngine;
using UnityEngine.UI;


namespace Study_Inventory
{
    public class InventorySlot : MonoBehaviour
    {
        //  Slot은 내부에 Content라고 부르는 Image GameObject를
        // 제어하는 기능을 가집니다.
        // - 외부에서 특정 아이템을 설정하거나, 빈 Slot으로 만들 수도 있습니다.
        // - Cursor Slot의 경우에는 마우스의 위치를 따라다녀야 함으로
        //  위치를 설정하는 기능도 필요합니다

        [field:SerializeField] public Image Content { get; private set; }

        private RectTransform rectTransform;
        private InventoryItem item;

        public bool IsEmpty => (item == null);
        // 위와 아래는 동일합니다
        //public bool IsEmpty { get { return item == null; } }

        public InventoryItem Item => item;
        // 위의 표현은 Item을 호출하면 멤버변수 item을 반환하겠다는
        // 람다식 표현입니다. 읽기 전용으로 반환되어서 Set은 불가합니다.

        private void Start()
        {
            SetItem(null); // 임시 초기화 코드

            rectTransform = GetComponent<RectTransform>();
            // 아래의 표현도 됩니다.
            //rectTransform = transform as RectTransform;
            // as : 타입을 캐스팅하는 키워드입니다. 
        }

        /// <summary>
        /// Null을 허용하는 함수입니다. Null 전달시 슬롯이 비워집니다.
        /// </summary>
        /// <param name="inputItem"></param>
        public void SetItem(InventoryItem inputItem)
        {
            // inputItem은 Null이 들어오거나, 실제 InventoryItem 객체가 들어옵니다
            item = inputItem;

            if(item == null) // item이 없는 경우
            {
                Content.sprite = null;      // 이미지의 sprite를 비워준다
                Content.enabled = false;    // 이미지의 활성화를 off한다.
            }
            else // item이 있는 경우
            {
                Content.sprite = item.Icon; // 이미지의 sprite를 item.Icon으로 설정
                Content.enabled = true;     // 이미지의 활성화를 on한다.
            }
        }

        // 외부에서 위치값을 입력받아서 Slot의 위치를 갱신합니다.
        // Cursor 슬롯 전용 함수입니다.
        public void SetPosition(Vector2 inputPosition)
        {
            rectTransform.anchoredPosition = inputPosition;
        }

    }
}

