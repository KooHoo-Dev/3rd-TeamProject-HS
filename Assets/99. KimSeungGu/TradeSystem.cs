using UnityEngine;
using Jay;
using TMPro;
using UnityEngine.UI;
using System.Collections;

namespace Study_Inventory
{
    public class TradeSystem : SingletonBase<TradeSystem>
    {
        public enum TradeType
        {
            Buy,
            Sell
        }

        public struct TradeInfo
        {
            public TradeType            TradeType;
            public InventorySlot        Start;
            public InventorySlot        End;
            public InventoryItem        Item;
        }

        // 원래는 묶어서(클래스화 해서) 쓰는것이 좋습니다
        // ex) TradePopupView
        [Header("PopUp Ref")]
        public GameObject   Root;
        public TMP_Text     ContentText;
        public Button       YesButton;
        public Button       NoButton;

        private bool isClick = false;
        private bool isPositiveClick = false; // Yes 버튼을 눌렀을때

        public void RequestTradeEvent(TradeInfo info)
        {
            // 코루틴을 실행시키는 함수
            StartCoroutine(TriggerTradeEvent(info));
        }

        private IEnumerator TriggerTradeEvent(TradeInfo info)
        {
            // 1. 팝업을 띄우고
            Root.SetActive(true);
            string tradeWord = 
                (info.TradeType == TradeType.Buy) ? "buy" : "sell";
            int price = 
                (info.TradeType == TradeType.Buy) ? info.Item.Fullness : info.Item.Fullness / 2;


            ContentText.SetText($"Would you like to {tradeWord} " +
                $"{info.Item.Name} : {price} ?");
            // 구매 : Would you like to buy discord : 100 ?
            // 판매 : Would you like to sell discord : 100 ?

            // 2. 사용자의 입력이 들어올때까지 대기
            // (나중가면 WaitUntil 쓰세요)
            /*while(true)
            {
                if (isClick) break;
                yield return null; // 다음 프레임까지 처리를 양보함
            }*/
            yield return new WaitUntil(()=>isClick);

            // 3. 선택 완료시에 입력에 따른 처리를 한다
            isClick = false;
            Root.SetActive(false);

            if(isPositiveClick == false) // No를 눌렀다면
            {
                RollBackSlot(info.Start, info.End); // 각 슬롯을 롤백시키고
                yield break;                        // 코루틴에서 탈출합니다
            }

            // 4. 거래처리할게 있다면 거래를 진행시킨다
            bool isSuccess = false;

            switch(info.TradeType)
            {
                case TradeType.Buy:
                    if(User.Instance.CanAfford(info.Item.Fullness) == false) // 유저가 돈이 없다면
                    {
                        isSuccess = false;
                        
                        Debug.Log($"[TradeEvent] {info.Item.Name} 구매 실패 : 잔액 부족" +
                            $"{User.Instance.Money} < {info.Item.Fullness}");
                    }
                    else
                    {
                        // 구매 성공 처리
                        User.Instance.DecreaseMoney(info.Item.Fullness);
                        isSuccess = true;

                        Debug.Log($"[TradeEvent] {info.Item.Name} 구매 성공. " +
                            $"잔액 : {User.Instance.Money}");
                    }
                    break;
                case TradeType.Sell:
                    int gain = info.Item.Fullness / 2;
                    User.Instance.IncreaseMoney(gain);
                    isSuccess = true;

                    Debug.Log($"[TradeEvent] {info.Item.Name} 판매 성공. " +
                          $"잔액 : {User.Instance.Money}");
                    break;
            }

            if (isSuccess == false) RollBackSlot(info.Start, info.End);
        }

        private void RollBackSlot(InventorySlot start, InventorySlot end)
        {
            // 거래를 롤백시킵니다. 거래는 Swap이 안됩니다.
            // end에 있는 item을 start 넣어주면 됩니다.
            start.SetItem(end.Item);
            end.SetItem(null);
        }

        public void OnClickButton(bool isPositive)
        {
            isClick = true;
            isPositiveClick = isPositive;
        }
    }
}


