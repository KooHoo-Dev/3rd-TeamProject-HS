using UnityEngine;
using UnityEngine.Serialization;

namespace WeGotOne.Fish_Interface
{   
    
    public class FishStateManager : MonoBehaviour, ICatchable
    {
        public enum BaitType
        {
            None,
            Player,
            SmallFish,
            MediumFish,
            LargeFish,
        }

        [SerializeField] private FishMovement fishMovement;
        public FishMovement FishMovement => fishMovement;
        
        [Header("저항도(Resistance) 설정")]
        [SerializeField] private float maxResistance = 20f;

        [Header("물 출입 쿨타임")] [SerializeField] [Tooltip("물속에 들어온 직후 다시 물밖으로 나가는 감지를 무시할 시간")]
        private float waterExitCooldown = 1.0f;
        
        // 물 감지 센서
        [SerializeField] private FishObstacleSensor obstacleSensor;
        
        // 물 탈출 피드백 함수 예외값을 위한 bool값.
        public bool IsGrabbed => currentState is FishCaughtState || currentState is FishExhaustedState;
        public float MaxResistance => maxResistance; // 저항도
        public float CurrentResistance { get; private set; }
        public string CurrentStateName
        {
            get
            {
                if (currentState is FishIdleState) return "Idle";
                if (currentState is FishPatrolState) return "Patrol";
                if (currentState is FishCaughtState) return "Caught";
                if (currentState is FishExhaustedState) return "Exhausted";
                if (currentState is FishFaintState) return "Faint";
                if (currentState is FishRunawayState) return "Runaway";
                if (currentState is AbductionState) return "Abduction";
                return "Unknown";
            }
        }
        
        public IFishState currentState;

        public Vector3 TargetPosition;
        
        public BaitType myType;     // 내 타입 (내가 대상일 경우 어떤 취급인지)
        public BaitType baitType;   // 먹이 타입 (내가 타겟으로 삼을 대상)
        public float Hunger;   // 배고픔 게이지 (이걸로 먹을지 말지 결정)
        public float HungerLimit;   // 이거 이하면 먹는거
        public float HungerAmount;   // 내가 먹히면 얼마나 채워주는지
        public FishStateManager BaitInfo;// 먹이 정보
        public PlayerController TargetInfo;// 먹이 정보
        
        // 문어 발사체 정보
        [SerializeField] private GameObject bulletPrefab;
        public GameObject BulletPrefab => bulletPrefab;
        
        private float lastEnteredWaterTime; // 마지막에 물에 들어온 시각
        
        public Transform PlayerRootPosition { get; set; }
        [Header("납치 기믹 설정")]
        [field: FormerlySerializedAs("<AbductionTarget>k__BackingField")]
        [field: SerializeField]
        public Transform AbductionDestination{ get; set; }
        
        // 기절 파티클용
        [SerializeField] public ParticleSystem confusedParticle;
        
        private void OnEnable()
        {
            fishMovement = GetComponent<FishMovement>();
            obstacleSensor = GetComponent<FishObstacleSensor>();
            if (obstacleSensor != null)
            {
                obstacleSensor.OnMovementBlocked += HandleWaterBoundary;
            }

            ResetResistance();
            maxResistance = 20f;
            waterExitCooldown = 0f;
            TargetPosition = Vector3.zero;
            BaitInfo = null;
            Hunger = Random.Range(60f, 100f);
            lastEnteredWaterTime = -1;
            
            ChangeState(new FishIdleState());
        }

        private void Update()
        {
            DecreaseFishHunger();
            currentState?.Tick();
        }

        private void HandleWaterBoundary()
        {
            if (currentState is FishPatrolState patrolState)
            {
                patrolState.SetNewWaypoint();
            }
        }
        
        private void OnTriggerEnter2D(Collider2D other)
        {
            // 현재 상태 먹이 추적 + other에 물고기 상태머신 잇으면
            if (currentState is FishBaitState && other.TryGetComponent(out FishStateManager fishState))
            {
                // 배고픔 관련. 다른 물고기 들어왔을때 배고프면 먹게
                if (fishState == BaitInfo && Hunger < HungerLimit)
                {
                    IncreaseFishHunger(fishState.HungerAmount);
                    BaitInfo = null;
                    ChangeState(new FishIdleState());
                }
            }
        }
        
        # region 배고픔 관련

        private void DecreaseFishHunger()
        {
            Hunger = Mathf.Max(0f, Hunger -= Time.deltaTime);
        }
        
        public void IncreaseFishHunger(float amount)
        {
            Hunger += amount;
        }

        public bool CanEat(FishStateManager fish)
        {
            if (fish.myType < baitType)
            {
                BaitInfo = fish;
                return true;
            }

            return false;
        }
        
        # endregion

        #region 저항도 제어 함수

        public void DecreaseResistance(float amount)
        {
            CurrentResistance = Mathf.Max(0f, CurrentResistance - amount);
        }

        public void ResetResistance()
        {
            CurrentResistance = maxResistance;
        }

        #endregion

        #region ICatchable 인터페이스 구현 (손 주도 상호작용)

        public void OnGrabbed()
        {
            // [기믹 반영]: 물고기가 물 위에서 기절(Faint)해 있을 때 다시 잡으면,
            // 저항 없이 즉시 탈진(Exhausted) 상태로 파지되게 처리합니다.
            if (currentState is FishFaintState)
            {
                TransitionTo(new FishExhaustedState());
            }
            else
            {
                // 정상 상태에서 잡으면 힘겨루기(Caught) 시작
                TransitionTo(new FishCaughtState());
            }
        }

        public void OnReleased()
        {
            float resistanceRatio = CurrentResistance / maxResistance;

            // 1. 저항력이 30% 미만으로 떨어진 상태에서 놓친 경우 (Exhausted 상태 포함)
            if (resistanceRatio < 0.3f)
            {
                // 물 위에서 일정 시간 누워있는 기절 상태로 돌입!
                TransitionTo(new FishFaintState());
            }
            // 2. 저항력이 아직 생생한 상태(60% 이상)에서 아쉽게 놓친 경우
            else if (resistanceRatio >= 0.6f)
            {
                TransitionTo(new FishRunawayState());
            }
            // 3. 회색 영역 (30% ~ 60%)
            else
            {
                TransitionTo(new FishRunawayState());
            }
        }

        public void OnMissed()
        {
            if (currentState is FishIdleState)
            {
                TransitionTo(new FishRunawayState());
            }
        }

        #endregion

        #region 비동기 FSM 코어

        public void ChangeState(IFishState newState)
        {
            TransitionTo(newState);
        }
        
        private void TransitionTo(IFishState newState)
        {   
            if (newState == null) return;

            currentState = newState;
            currentState.Enter(this);
        }
        
        public void OnPlayerCaught(Transform playerTransform)
        {
            // 이미 납치 중이 아닐 때만 실행
            if (!(currentState is AbductionState))
            {
                PlayerRootPosition = playerTransform;
        
                // TODO: 플레이어의 조작을 제한하는 로직 호출 (예: playerTransform.GetComponent<PlayerController>().Lock())
        
                // 납치 상태로 전환
                ChangeState(new AbductionState()); 
            }
        }

        #endregion
        
        # region 상태이상 파티클
        // 파티클 켜주는 함수
        public void PlayConfusedParticle()
        {
            // 파티클 없으면 리턴
            if (confusedParticle == null)
                return;
            // 파티클 실행중이면 리턴 (켜져있는데 또 켜면 어색함)
            if (confusedParticle.isPlaying)
                return;
            
            // 안켜져있을때만 켜는거에요
            confusedParticle.gameObject.SetActive(true);
        }
        
        public void StopConfusedParticle()
        {
            // 파티클 없으면 리턴
            if (confusedParticle == null)
                return;
            
            // 안켜져있을때만 켜는거에요
            confusedParticle.gameObject.SetActive(false);
        }
        
        # endregion
    }
}
