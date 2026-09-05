using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using HelloServer;

namespace WeGotOne
{
    public class PlayerStamina : MonoBehaviour
    {
        [SerializeField] private Transform spawnPoint;
        
        [Header("스태미나 기본 설정")]
        [SerializeField] private float maxStamina = 100f;
        [SerializeField] private float currentStamina = 100f;
        
        // 무게에 따른 추가 배율
        [SerializeField] private PlayerController playerController;
        [SerializeField] private float forceStaminaMultiplier = 0.1f;

        [SerializeField] private float recoverDelay = 5f;
        [SerializeField] private float recoverPerSecond = 10f;

        private float outOfWaterTime;
        
        [Header("제한사항")]
        [SerializeField] private float staminaTime = 100f;
        private int waterLayer;
        private bool isInWater = false;
        
        [Header("스태미나 감소 배율 : 혹시 몰라서 확장해둠")]
        [SerializeField] private float staminaDecreaseMultiplier = 1f;

        // 플레이어 무력화,활성화시 게임 매니저에게 알려주는 델리게이트
        public event Action<bool> OnPlayerStarveStateChanged;
        
        // 외부 확인용 UI 띄울때 이걸루 접근
        [SerializeField] public Image StaminaGauge;
        public float CurrentStamina => currentStamina;
        public float MaxStamina => maxStamina;
        private float staminaSoundTimer;
        
        // 움직임 제어용
        public bool CanMove => currentStamina > 0f;
        public bool IsStarved => CanMove == false;

        #region unity events

        private void Update()
        {
            if (CanRunLocalGameLogic)
            {
                if (isInWater)
                {
                    outOfWaterTime = 0f;
                    DecreaseStaminaOverTime();
                }
                else
                {
                    RecoverStaminaOverTime();
                }
            }

            UpdateStaminaUI();
        }

        private void OnEnable()
        {
            GameManager.Instance.SubscribePlayerStarveStateChanged(this);
            waterLayer = LayerMask.NameToLayer("Water");
        }
        
        private void OnDisable()
        {
            GameManager.Instance.UnsubscribeStarveStateChanged(this);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.gameObject.layer == waterLayer)
            {
                isInWater = true;
            }
        }
        
        private void OnTriggerExit2D(Collider2D other)
        {
            if (other.gameObject.layer == waterLayer)
            {
                isInWater = false;
            }
        }

        #endregion

        public void SetSpawnPoint(Transform spawnPoint)
        {
            this.spawnPoint = spawnPoint;
        }

        private void UpdateStaminaUI()
        {
            if (StaminaGauge == null) return;
            
            staminaSoundTimer += Time.deltaTime;

            float interval = GetStaminaUpdateInterval();

            if (staminaSoundTimer < interval)
            {
                return;
            }

            staminaSoundTimer = 0f;

            StaminaGauge.transform.parent.DOPunchScale(Vector3.one * 0.2f, 0.3f);
            AudioManager.Instance.PlaySfx(AudioManager.Sfx.Pulse);
        }
        
        private float GetStaminaUpdateInterval()
        {
            float staminaPercent = currentStamina / maxStamina;
            
            if (staminaPercent >= 0.5f)
            {
                return 5f;
            }
            if (staminaPercent >= 0.3f)
            {
                return 3f;
            }
            
            return 1f;
        }

        private void DecreaseStaminaOverTime()
        {
            if (currentStamina <= 0f)
                return;

            if (isInWater == false)
                return;
            
            // 기본 스태미나 감소량
            float baseDecreasePerSecond = maxStamina / staminaTime;
            float highestForce = playerController.GetHighestHungerForce();
            float decreaseAmount = (baseDecreasePerSecond + (highestForce * forceStaminaMultiplier)) * staminaDecreaseMultiplier * Time.deltaTime;
            
            DecreaseStamina(decreaseAmount);
            
            UpdateStamina();
        }
        
        // 나중에 이벤트가 불러올 함수
        public void SetDecreaseMultiplier(float multiplier)
        {
            staminaDecreaseMultiplier = multiplier;
        }
        
        /// <summary>
        /// amount만큼 스태미나 게이지 증가
        /// </summary>
        /// <param name="amount"></param>
        public void IncreaseStamina(float amount)
        {
            if (CanRunLocalGameLogic == false) return;

            bool wasStarved = IsStarved;
            currentStamina = Mathf.Min( currentStamina + amount, maxStamina);

            // 이전에 굶주린 상태였다가 회복이 되면
            if (wasStarved && IsStarved == false)
            {
                // 게임매니저에 통지
                NotifyPlayerStarveStateChanged( false);
            }

            UpdateStamina();
        }

        /// <summary>
        /// amount만큼 스태미나 게이지 감소
        /// </summary>
        /// <param name="amount"></param>
        public void DecreaseStamina(float amount)
        {
            if (CanRunLocalGameLogic == false) return;

            bool wasStarved = IsStarved;
            currentStamina = Mathf.Max( currentStamina - amount, 0f );
            
            // 만약 이전에 굶주리지 않았다가 이번 스태미나 감소시 굶주리게 되었다면
            if (wasStarved == false && IsStarved)
            {
                if (spawnPoint == null)
                {
                    Debug.LogWarning("스테미나업슴");
                    return;
                }

                transform.position = spawnPoint.position;
                
                // 델리게이트로 게임매니저로 통지
                NotifyPlayerStarveStateChanged( true);
            }
        }
        
        private void RecoverStaminaOverTime()
        {
            // 최대 스태미나 넘기기
            if (currentStamina >= maxStamina)
                return;

            // 카운트 세기
            outOfWaterTime += Time.deltaTime;

            // 5초전에 넘기기
            if (outOfWaterTime < recoverDelay)
                return;

            // 회복 시작
            float recoverAmount = recoverPerSecond * Time.deltaTime;

            IncreaseStamina(recoverAmount);
        }

        private void UpdateStamina()
        {
            // 업데이트가 먼저 돌아버려서 그냥 널처리 햇어요
            if (StaminaGauge == null)
            {
                return;
            }
            
            StaminaGauge.fillAmount = currentStamina / maxStamina;
        }

        public void NotifyPlayerStarveStateChanged(bool isStarved)
        {
            if (CanRunLocalGameLogic == false) return;

            OnPlayerStarveStateChanged?.Invoke(isStarved);
        }

        public void ApplySnapshot(float snapshotCurrentStamina)
        {
            currentStamina = Mathf.Clamp(snapshotCurrentStamina, 0f, maxStamina);
            UpdateStamina();
        }

        private bool CanRunLocalGameLogic
        {
            get
            {
                return RoomClient.IsInitialized == false ||
                       RoomClient.Instance.Me == null ||
                       RoomClient.Instance.Me.IsHost;
            }
        }

        // GameoverTester를 위한 테스트 메소드
        #region test methods

        public void ResetStamina()
        {
            IncreaseStamina(maxStamina);
        }

        public void MakeStarved()
        {
            DecreaseStamina(maxStamina);
        }

        #endregion
    }    
}