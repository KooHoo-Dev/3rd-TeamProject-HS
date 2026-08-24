using UnityEngine;

namespace WeGotOne
{
    public class PlayerHunger : MonoBehaviour
    {
        [Header("배고픔 기본 설정")]
        [SerializeField] private float maxHunger = 100f;
        [SerializeField] private float currentHunger = 100f;
        
        [Header("제한시간 : 추후 낮밤 시간 합으로 변경 가능성 잇음")]
        [SerializeField] private float hungerTime = 150f;
        
        [Header("허기 감소 배율 : 혹시 몰라서 확장해둠")]
        [SerializeField] private float hungerDecreaseMultiplier = 1f;
        
        // 외부 확인용 UI 띄울때 이걸루 접근
        public float CurrentHunger => currentHunger;
        public float MaxHunger => maxHunger;
        
        // 움직임 제어용
        public bool CanMove => currentHunger > 0f;

        private void Update()
        {
            DecreaseHungerOverTime();
        }

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
            currentHunger = Mathf.Min( currentHunger + amount, maxHunger);
        }
        
        /// <summary>
        /// amount만큼 허기 게이지 감소
        /// </summary>
        /// <param name="amount"></param>
        public void DecreaseHunger(float amount)
        {
            currentHunger = Mathf.Max( currentHunger - amount, 0f );
        }
    }    
}