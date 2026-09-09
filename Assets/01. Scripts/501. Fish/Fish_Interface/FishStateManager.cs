using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace WeGotOne.Fish_Interface
{
    public class FishStateManager : MonoBehaviour, ICatchable
    {
        public enum BaitType
        {
            Player,
            SmallFish,
            MediumFish,
            LargeFish,
            None,
        }

        [Header("물고기 이름")]
        [field: SerializeField]
        public string FishName { get; private set; }

        public FishObjectPool ObjectPool;

        [SerializeField] private FishMovement fishMovement;
        public WorldItem WorldItem { get; private set; }
        public FishMovement FishMovement => fishMovement;

        [Header("저항도(Resistance) 설정")]
        [SerializeField]
        private float maxResistance = 20f;

        // 물 감지 센서
        [SerializeField] private FishObstacleSensor obstacleSensor;
        public FishObstacleSensor ObstacleSensor => obstacleSensor;

        // 물 탈출 피드백 함수 예외값을 위한 bool값.
        public bool IsGrabbed => CurrentState is FishCaughtState || CurrentState is FishExhaustedState;
        public float MaxResistance => maxResistance; // 저항도
        [Header("현재 저항도")]
        [field: SerializeField] public float CurrentResistance { get; private set; }
        [field: SerializeField] public ParticleSystem HitParticle { get; private set; }

        public string CurrentStateName
        {
            get
            {
                if (CurrentState is FishIdleState) return "Idle";
                if (CurrentState is FishPatrolState) return "Patrol";
                if (CurrentState is FishCaughtState) return "Caught";
                if (CurrentState is FishExhaustedState) return "Exhausted";
                if (CurrentState is FishFaintState) return "Faint";
                if (CurrentState is FishRunawayState) return "Runaway";
                if (CurrentState is AbductionState) return "Abduction";
                if (CurrentState is FishItemState) return "FishItem";
                return "Unknown";
            }
        }

        public IFishState CurrentState;

        public Vector3 TargetPosition;

        [Header("먹이 정보")]
        public BaitType myType; // 내 타입 (내가 대상일 경우 어떤 취급인지)
        public BaitType baitType; // 먹이 타입 (내가 타겟으로 삼을 대상)
        public float Hunger; // 배고픔 게이지 (이걸로 먹을지 말지 결정)
        public float HungerLimit; // 이거 이하면 먹는거
        public float HungerAmount; // 내가 먹히면 얼마나 채워주는지
        public FishStateManager BaitInfo; // 먹이 정보
        public PlayerController TargetInfo; // 플레이어 정보

        [Header("문어 발사체 정보")]
        [SerializeField] private GameObject bulletPrefab;
        public GameObject BulletPrefab => bulletPrefab;

        [Header("납치 기믹 설정")] // 납치 상태 설정
        // 납치 할 장소
        [field: SerializeField] public Transform AbductionDestination { get; set; } 
        // 플레이어를 어디에 달고 다닐지
        [field: SerializeField] public Transform PlayerAttachPosition { get; set; }
        public Transform PlayerRootPosition { get; set; }
        public Vector2 StartPosition { get; set; }

        [Header("공격 가능 여부")]
        public bool IsAttack = true;
        [Header("기절 쿨타임")]
        public float FaintDuration;

        // 기절 파티클용
        [SerializeField] public ParticleSystem confusedParticle;
        [field: SerializeField] public GameObject RunawayParticle { get; set; }
        
        public float StartPosY { get; set; }
        
        public Animator Animator { get; set; }
        
        private readonly HashSet<HandControllerV2> grabbingHands = new HashSet<HandControllerV2>();

        private void Awake()
        {
            WorldItem = GetComponent<WorldItem>();
            Animator = GetComponent<Animator>();
        }

        private void OnEnable()
        {
            grabbingHands.Clear();

            StartPosition = transform.position;
            fishMovement = GetComponent<FishMovement>();
            obstacleSensor = GetComponent<FishObstacleSensor>();
            if (obstacleSensor != null)
            {
                obstacleSensor.OnMovementBlocked += HandleWaterBoundary;
            }

            ResetResistance();
            StopConfusedParticle();
            TargetPosition = Vector3.zero;
            BaitInfo = null;
            Hunger = Random.Range(60f, 100f);
            FishName = gameObject.name;
            
            switch (WorldItem.ItemId)
            {
                case 4: AbductionDestination = GameObject.Find("Squid Destination").transform;
                    break;
                case 9: AbductionDestination = GameObject.Find("Seahorse Destination").transform;
                    break;
            }

            ObjectPool = FindFirstObjectByType<FishObjectPool>();
            ChangeState(new FishIdleState());
            
        }

        private void Start()
        {
            StartPosY = transform.position.y;
        }

        private void Update()
        {
            DecreaseFishHunger();
            CurrentState?.Tick();
        }

        private void OnDisable()
        {
            CurrentState.Exit();
        }

        private void HandleWaterBoundary(RaycastHit2D obstacleHit)
        {
            if (CurrentState is FishPatrolState patrolState)
            {
                patrolState.SetNewWaypoint(obstacleHit);
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            // 현재 상태 먹이 추적 + other에 물고기 상태머신 잇으면
            if (CurrentState is FishBaitState && other.TryGetComponent(out FishStateManager fishState))
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

        public void OnGrabbed(HandControllerV2 hand) // 시그니처 수정
        {
            if (hand != null)
            {
                grabbingHands.Add(hand); // 추가: 잡은 손 등록
            }

            if (FishName.Contains("Seahorse"))
            {
                if (!(CurrentState is AbductionState))
                {
                    ChangeState(new AbductionState());
                }
                return;
            }

            // 이미 잡혀서 힘겨루기/탈진 중이라면 다른 손이 추가로 잡아도 상태 재진입 방지
            if (CurrentState is FishCaughtState || CurrentState is FishExhaustedState)
                return;

            if (CurrentState is FishFaintState)
            {
                TransitionTo(new FishExhaustedState());
            }
            else
            {
                TransitionTo(new FishCaughtState());
            }
        }

        public void OnReleased(HandControllerV2 hand) // 시그니처 수정
        {
            if (hand != null)
            {
                grabbingHands.Remove(hand); // 추가: 놓은 손 제거
            }

            // [핵심]: 아직 다른 손(본인의 반대 손이든, 다른 플레이어의 손이든)이 잡고 있다면 즉시 리턴
            if (grabbingHands.Count > 0)
                return;

            // --- 아래는 모든 손이 완전히 놓였을 때(grabbingHands.Count == 0)만 실행 ---

            if (FishName.Contains("Seahorse"))
            {
                CurrentState.Exit();
                return;
            }

            float resistanceRatio = CurrentResistance / maxResistance;

            if (resistanceRatio < 0.3f)
            {
                TransitionTo(new FishFaintState());
            }
            else
            {
                TransitionTo(new FishRunawayState());
            }
        }

        public void OnMissed()
        {
            if (CurrentState is FishIdleState)
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

            CurrentState = newState;
            CurrentState.Enter(this);
        }

        public void OnPlayerCaught(Transform playerTransform)
        {
            // 이미 납치 중이 아닐 때만 실행
            if (!(CurrentState is AbductionState))
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