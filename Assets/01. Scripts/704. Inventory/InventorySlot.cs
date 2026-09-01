using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


namespace WeGotOne
{
    public class InventorySlot : MonoBehaviour
    {
        //  Slot은 내부에 Content라고 부르는 Image GameObject를
        // 제어하는 기능을 가집니다.
        // - 외부에서 특정 아이템을 설정하거나, 빈 Slot으로 만들 수도 있습니다.
        // - Cursor Slot의 경우에는 마우스의 위치를 따라다녀야 함으로
        //  위치를 설정하는 기능도 필요합니다

        [field: SerializeField] public Image Content { get; private set; }
        [field: SerializeField] public TMP_Text ItemCountText { get; private set; }

        private RectTransform rectTransform;
        private ItemScriptableObject item;

        public bool IsEmpty => (item == null);
        // 위와 아래는 동일합니다
        //public bool IsEmpty { get { return item == null; } }

        public ItemScriptableObject Item => item;
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

        public void CountTextUpdate(int itemKey, bool isPlus)
        {
            // 지금 코드가 인벤토리 db에 영향을 주려면 db를 직접 건드는게 아니라 UI 객체인 InventorySlot을 거치는 구조라서
            // 나중에 유지보수할때 '어? ItemDb를 안건드리고 왜 UI 오브젝트를 건들지?' 라고 오해할 수 있을 거 같습니다.
            // 지금 BridgeManager -> InventorySlot -> InventoryItemDB 에 값 업데이트인데
            // 나중에
            // 1. BridgeManager -> InventoryDB
            // 2. InventorySlot ---> InventoryDB (Inventory나 InventorySlot이 InventoryDB 참조해서 업데이트) 
            // 델리게이트나 Slot이 DB의 값을 참조하는 형태 둘 중 하나로 리팩토링해야할 거 같아요
            
            // 지금은 괜찮은데 나중에 아래와 같은 문제가 있으면 꼭 리팩토링 해야할 거 같습니다
            // 1. 서버/저장/초기화에서 UI 없이 Inventory를 변경해야 하거나 (유니티 씬 로딩 같이)
            // 2. InventorySlot이 UI 외 책임을 계속 흡수하면서 커지는 문제 (UI, DB 문제 터지면 의도치 않게 이 코드도 봐야할 수도 있음. InventorySlot인데 실제로는 InventoryDB 개수 관리 책임도 가지고 있으니까 )
            int itemCount = InventoryItemDB.Instance.ItemCountDic[itemKey];
            
            itemCount += isPlus ? 1 : -1;
            InventoryItemDB.Instance.ItemCountDic[itemKey] = itemCount;
            ItemCountText.text = itemCount.ToString();
            transform.DOPunchScale(Vector3.one * 0.3f, 0.3f);

            if (itemCount <= 0)
            {
                SetItem(null);
            }
        }

        public void SetItemCountFromSnapshot(int itemCount)
        {
            ItemCountText.text = itemCount.ToString();
        }

        /// <summary>
        /// Null을 허용하는 함수입니다. Null 전달시 슬롯이 비워집니다.
        /// </summary>
        /// <param name="inputItem"></param>
        public void SetItem(ItemScriptableObject inputItem)
        {
            // inputItem은 Null이 들어오거나, 실제 InventoryItem 객체가 들어옵니다
            item = inputItem;

            if (item == null) // item이 없는 경우
            {
                Content.sprite = null; // 이미지의 sprite를 비워준다
                Content.enabled = false; // 이미지의 활성화를 off한다.
                ItemCountText.enabled = false;
            }
            else // item이 있는 경우
            {
                Content.sprite = item.Icon; // 이미지의 sprite를 item.Icon으로 설정
                Content.enabled = true; // 이미지의 활성화를 on한다.
                ItemCountText.enabled = true;
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
