using System;
using UnityEngine;
using UnityEngine.UI;

namespace WeGotOne
{
    public class PlayerHunger : MonoBehaviour
    {
        [Header("배고픔 기본 설정")]
        [SerializeField] private float maxHunger = 100f;
        [SerializeField] private float currentHunger = 100f;
        
        [Header("제한시간")]
        [SerializeField] private float hungerTime = 150f;
        
        [Header("허기 감소 배율 : 혹시 몰라서 확장해둠")]
        [SerializeField] private float hungerDecreaseMultiplier = 1f;
        
        [SerializeField] private Image hungerGauge;
        
        // 플레이어 무력화,활성화시 게임 매니저에게 알려주는 델리게이트
        public event Action<bool> OnPlayerStarveStateChanged;
        
        // 외부 확인용 UI 띄울때 이걸루 접근
        public float CurrentHunger => currentHunger;
        public float MaxHunger => maxHunger;
        
        // 움직임 제어용
        public bool CanMove => currentHunger > 0f;
        public bool IsStarved => CanMove == false;

        #region unity events

        private void Update()
        {
            DecreaseHungerOverTime();
        }

        private void OnEnable()
        {
            GameManager.Instance.SubscribePlayerStarveStateChanged(this);
        }
        
        private void OnDisable()
        {
            GameManager.Instance.UnsubscribeStarveStateChanged(this);
        }

        #endregion

        private void DecreaseHungerOverTime()
        {
            if (currentHunger <= 0f)
                return;

            // 기본 배고픔 감소량
            float baseDecreasePerSecond = maxHunger / hungerTime;
            
            // 나중에 배고픔 덜달거나 더다는 이벤트 넣을꺼면 멀티플러만 조절해주는 용도.
            // 이벤트 추가 안되면 그냥 베이스에 델타 타임만 곱할꺼임
            float decreaseAmount = baseDecreasePerSecond * hungerDecreaseMultiplier * Time.deltaTime;

            DecreaseHunger(decreaseAmount);
            
            // UpdateHunger();
        }
        
        // 나중에 이벤트가 불러올 함수
        public void SetDecreaseMultiplier(float multiplier)
        {
            hungerDecreaseMultiplier = multiplier;
        }
        
        /// <summary>
        /// amount만큼 허기 게이지 증가
        /// </summary>
        /// <param name="amount"></param>
        public void IncreaseHunger(float amount)
        {
            bool prevIsStarved = !CanMove;
            currentHunger = Mathf.Min( currentHunger + amount, maxHunger);

            // 이전에 굶주린 상태였다가 회복이 되면
            if (prevIsStarved)
            {
                // 게임매니저에 통지
                NotifyPlayerStarveStateChanged( false);
            }

            // UpdateHunger();
        }

        /// <summary>
        /// amount만큼 허기 게이지 감소
        /// </summary>
        /// <param name="amount"></param>
        public void DecreaseHunger(float amount)
        {
            currentHunger = Mathf.Max( currentHunger - amount, 0f );
            
            // 만약 이전에 굶주리지 않았다가 이번 배고픔 감소시 굶주리게 되었다면
            if (CanMove == false)
            {
                // 델리게이트로 게임매니저로 통지
                NotifyPlayerStarveStateChanged( true);
            }
        }

        private void UpdateHunger()
        {
            //hungerGauge.fillAmount = currentHunger / maxHunger;
        }

        public void NotifyPlayerStarveStateChanged(bool isStarved)
        {
            OnPlayerStarveStateChanged?.Invoke(isStarved);
        }

        // GameoverTester를 위한 테스트 메소드
        #region test methods

        public void ResetHunger()
        {
            IncreaseHunger(maxHunger);
        }

        public void MakeStarved()
        {
            DecreaseHunger(maxHunger);
        }

        #endregion
    }    
}