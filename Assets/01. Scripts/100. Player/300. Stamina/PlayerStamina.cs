using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using HelloServer;


namespace WeGotOne
{
    public class PlayerStamina : MonoBehaviour
    {
        [SerializeField] private Vector3 spawnPosition;

        [Header("스태미나 기본 설정")]
        [SerializeField] private float maxStamina = 500f;
        [SerializeField] private float currentStamina = 500f;

        // 무게에 따른 추가 배율
        [SerializeField] private PlayerController playerController;
        [SerializeField] private float forceStaminaMultiplier = 0.01f;

        [SerializeField] private float recoverDelay = 5f;
        [SerializeField] private float recoverPerSecond = 50f;

        private float outOfWaterTime;

        [Header("제한사항")]
        [SerializeField] private float staminaTime = 150f;

        private int waterLayer;
        private int safeAreaLayer;

        // 물과 안전구역 Collider가 여러 개 겹쳐도 정확하게 감지하기 위한 카운터
        private int waterTriggerCount;
        private int safeAreaTriggerCount;

        private bool IsInWater => waterTriggerCount > 0;
        private bool IsInSafeArea => safeAreaTriggerCount > 0;

        [Header("스태미나 감소 배율 : 혹시 몰라서 확장해둠")]
        [SerializeField] private float staminaDecreaseMultiplier = 1f;

        // 플레이어 무력화, 활성화시 게임 매니저에게 알려주는 델리게이트
        public event Action<bool> OnPlayerStarveStateChanged;

        // 외부 확인용 UI 띄울때 이걸로 접근
        [SerializeField] public Image StaminaGauge;

        public float CurrentStamina => currentStamina;
        public float MaxStamina => maxStamina;

        private float staminaSoundTimer;

        public GameObject up;
        public GameObject down;

        // 움직임 제어용
        public bool CanMove => currentStamina > 0f;
        public bool IsStarved => CanMove == false;

        #region Unity Events

        private void Update()
        {
            float beforeStamina = currentStamina;

            if (CanRunLocalGameLogic)
            {
                // 물 안에 있으면서 안전구역이 아닌 경우에만 스태미나 감소
                if (IsInWater && IsInSafeArea == false)
                {
                    outOfWaterTime = 0f;
                    DecreaseStaminaOverTime();
                }
                else
                {
                    // 물 밖 또는 물 안의 안전구역에서는 스태미나 회복
                    RecoverStaminaOverTime();
                }
            }

            UpdateStaminaDirection(beforeStamina);

            UpdateStaminaUI();
        }

        private void OnEnable()
        {
            GameManager.Instance.SubscribePlayerStarveStateChanged(this);

            waterLayer = LayerMask.NameToLayer("Water");
            safeAreaLayer = LayerMask.NameToLayer("SafeArea");

            waterTriggerCount = 0;
            safeAreaTriggerCount = 0;
        }

        private void OnDisable()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.UnsubscribeStarveStateChanged(this);
            }

            waterTriggerCount = 0;
            safeAreaTriggerCount = 0;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.gameObject.layer == waterLayer)
            {
                waterTriggerCount++;
            }

            if (other.gameObject.layer == safeAreaLayer)
            {
                safeAreaTriggerCount++;
            }
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (other.gameObject.layer == waterLayer)
            {
                waterTriggerCount = Mathf.Max(waterTriggerCount - 1, 0);
            }

            if (other.gameObject.layer == safeAreaLayer)
            {
                safeAreaTriggerCount = Mathf.Max(safeAreaTriggerCount - 1, 0);
            }
        }

        #endregion

        #region Spawn

        public void SetSpawnPoint(Transform spawnPoint)
        {
            spawnPosition = new Vector3(
                spawnPoint.position.x,
                spawnPoint.position.y + 5f,
                spawnPoint.position.z
            );
        }

        public void GoSpawnPoint()
        {
            transform.position = spawnPosition;
        }

        #endregion

        #region Stamina UI

        private void UpdateStaminaUI()
        {
            if (StaminaGauge == null)
            {
                return;
            }

            staminaSoundTimer += Time.deltaTime;

            float interval = GetStaminaUpdateInterval();

            if (staminaSoundTimer < interval)
            {
                return;
            }

            staminaSoundTimer = 0f;

            StaminaGauge.transform.parent.DOPunchScale(
                Vector3.one * 0.2f,
                0.3f
            );

            if (AudioManager.Instance.sfxClips[(int)AudioManager.Sfx.Pulse])
            {
                AudioManager.Instance.PlaySfx(AudioManager.Sfx.Pulse);
            }
        }

        private void UpdateStaminaDirection(float beforeStamina)
        {
            if (up != null)
            {
                up.SetActive(currentStamina > beforeStamina);
            }

            if (down != null)
            {
                down.SetActive(currentStamina < beforeStamina);
            }
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

        #endregion

        #region Stamina Decrease

        private void DecreaseStaminaOverTime()
        {
            if (currentStamina <= 0f)
            {
                return;
            }

            // 물 밖에서는 감소하지 않음
            if (IsInWater == false)
            {
                return;
            }

            // 안전구역에서는 감소하지 않음
            if (IsInSafeArea)
            {
                return;
            }

            // 기본 스태미나 감소량
            float baseDecreasePerSecond = maxStamina / staminaTime;

            float highestForce = playerController.GetHighestHungerForce();

            float decreaseAmount =
                (baseDecreasePerSecond +
                (highestForce * forceStaminaMultiplier)) *
                staminaDecreaseMultiplier *
                Time.deltaTime;

            DecreaseStamina(decreaseAmount);

            UpdateStamina();
        }

        // 나중에 이벤트가 불러올 함수
        public void SetDecreaseMultiplier(float multiplier)
        {
            staminaDecreaseMultiplier = multiplier;
        }

        #endregion

        #region Stamina Increase

        /// <summary>
        /// amount만큼 스태미나 게이지 증가
        /// </summary>
        /// <param name="amount"></param>
        public void IncreaseStamina(float amount)
        {
            if (CanRunLocalGameLogic == false)
            {
                return;
            }

            bool wasStarved = IsStarved;

            // 디버그로 최대치를 넘긴 상태에서도 스태미나가 줄지 않도록 처리
            currentStamina = Mathf.Min(
                currentStamina + amount,
                Mathf.Max(currentStamina, maxStamina)
            );

            // 이전에 굶주린 상태였다가 회복되면
            if (wasStarved && IsStarved == false)
            {
                // 게임매니저에 통지
                NotifyPlayerStarveStateChanged(false);
            }

            UpdateStamina();
        }

        #endregion

        #region Stamina Decrease

        /// <summary>
        /// amount만큼 스태미나 게이지 감소
        /// </summary>
        /// <param name="amount"></param>
        public void DecreaseStamina(float amount)
        {
            if (CanRunLocalGameLogic == false)
            {
                return;
            }

            bool wasStarved = IsStarved;

            currentStamina = Mathf.Max(
                currentStamina - amount,
                0f
            );

            // 만약 이전에는 굶주리지 않았지만
            // 이번 스태미나 감소로 굶주리게 되었다면
            if (wasStarved == false && IsStarved)
            {
                StartCoroutine(playerController.Death());

                // 델리게이트로 게임매니저에 통지
                NotifyPlayerStarveStateChanged(true);
            }
        }

        #endregion

        #region Stamina Recovery

        private void RecoverStaminaOverTime()
        {
            // 최대 스태미나라면 회복하지 않음
            if (currentStamina >= maxStamina)
            {
                return;
            }

            // 회복 대기시간 카운트
            outOfWaterTime += Time.deltaTime;

            // 회복 대기시간 전이라면 대기
            if (outOfWaterTime < recoverDelay)
            {
                return;
            }

            // 회복 시작
            float recoverAmount = recoverPerSecond * Time.deltaTime;

            IncreaseStamina(recoverAmount);
        }

        #endregion

        #region Stamina Update

        private void UpdateStamina()
        {
            if (StaminaGauge == null)
            {
                return;
            }

            StaminaGauge.fillAmount = currentStamina / maxStamina;
        }

        public void NotifyPlayerStarveStateChanged(bool isStarved)
        {
            if (CanRunLocalGameLogic == false)
            {
                return;
            }

            OnPlayerStarveStateChanged?.Invoke(isStarved);
        }

        public void ApplySnapshot(float snapshotCurrentStamina)
        {
            // 호스트가 설정한 최대치 초과 디버그 스태미나도 그대로 동기화
            currentStamina = Mathf.Max(
                snapshotCurrentStamina,
                0f
            );

            UpdateStamina();
        }

        #endregion

        #region Local Game Logic

        private bool CanRunLocalGameLogic
        {
            get
            {
                return RoomClient.IsInitialized == false ||
                       RoomClient.Instance.Me == null ||
                       RoomClient.Instance.Me.IsHost;
            }
        }

        #endregion

        #region Test Methods

        // GameoverTester를 위한 테스트 메소드
        public void SetDebugStamina(float value)
        {
            if (CanRunLocalGameLogic == false)
            {
                return;
            }

            bool wasStarved = IsStarved;

            currentStamina = value;

            if (wasStarved != IsStarved)
            {
                NotifyPlayerStarveStateChanged(IsStarved);
            }

            UpdateStamina();
        }

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