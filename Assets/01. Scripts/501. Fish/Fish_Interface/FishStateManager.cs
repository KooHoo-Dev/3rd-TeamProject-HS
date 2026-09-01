using System;
using System.Threading;
using UnityEngine;

namespace WeGotOne.Fish_Interface
{   
    
    public class FishStateManager : MonoBehaviour, ICatchable
    {
        [SerializeField] private FishMovement fishMovement;
        public FishMovement FishMovement => fishMovement;
        
        [Header("저항도(Resistance) 설정")]
        [SerializeField] private float maxResistance = 20f;

        [Header("물 출입 쿨타임")] [SerializeField] [Tooltip("물속에 들어온 직후 다시 물밖으로 나가는 감지를 무시할 시간")]
        private float waterExitCooldown = 1.0f;
        
        // 물 탈출 피드백 함수 예외값을 위한 bool값.
        public bool IsGrabbed => currentState is FishCaughtState || currentState is FishExhaustedState;
        public float MaxResistance => maxResistance; // 저항도
        public float CurrentResistance { get; private set; }
        
        private WaterLayerAlert waterLayerAlert; // 물 탈출 알리미
        public IFishState currentState;

        public Vector3 TargetPosition;

        private float lastEnteredWaterTime = -999f; // 마지막에 물에 들어온 시각
        
        public Transform PlayerRootPosition { get; set; }
        [Header("납치 기믹 설정")]
        [field: SerializeField]
        public Transform AbductionTarget{ get; set; }
        
        // 기절 파티클용
        [SerializeField] public ParticleSystem confusedParticle;

        private void Awake()
        {
            fishMovement = GetComponent<FishMovement>();
            
            waterLayerAlert = GetComponent<WaterLayerAlert>();
            if (waterLayerAlert != null)
            {
                waterLayerAlert.OnEnteredWater += HandleEnteredWater;
                waterLayerAlert.OnExitedWater += HandleExitedWater;
            }
        }

        private void Start()
        {
            ResetResistance();
            ChangeState(new FishIdleState());
        }

        private void Update()
        {
            currentState?.Tick();
            Debug.Log(currentState);
        }

        private void OnDisable()
        {
            if (waterLayerAlert != null)
            {
                waterLayerAlert.OnEnteredWater -= HandleEnteredWater;
                waterLayerAlert.OnExitedWater -= HandleExitedWater;
            }
        }

        #region 물 출입 시 실행할 예외 판단 및 위임 함수

        private void HandleExitedWater()
        {
            if (Time.time - lastEnteredWaterTime < waterExitCooldown)
            {
                Debug.Log($"{name} 물 속 진입 쿨타임: 물 밖 판정 무시");
                return;
            }
            
            // 플레이어 손에 안 잡혀 있을 때만 물리 처리 위임
            if (!IsGrabbed && fishMovement != null)
            {
                fishMovement.SetAirPhysics();
            }
        }

        private void HandleEnteredWater()
        {
            lastEnteredWaterTime = Time.time;
            
            // 플레이어 손에 안 잡혀 있을 때만 물리 복구 위임
            if (!IsGrabbed && fishMovement != null)
            {
                fishMovement.SetWaterPhysics();
            }
        }
        
        #endregion
        
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

            if (currentState != null)
            {
                currentState.ExitAsync();
            }

            currentState = newState;
            currentState.EnterAsync(this);
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

        public void OnFloorHit()
        {
            // 현재 납치 상태라면 바닥에 닿았을 때 상태 종료 (예: 순찰 복귀 상태로 전환)
            if (currentState is AbductionState abductionState)
            {
                abductionState.HandleFloorHit();
            }
        }

        #endregion

        #region 파티클 켜주는 함수

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

        #endregion
        
    }
}