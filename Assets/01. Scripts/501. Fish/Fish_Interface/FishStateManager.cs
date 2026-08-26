using System;
using System.Threading;
using UnityEngine;

namespace WeGotOne.Fish_Interface
{   
    [RequireComponent(typeof(Rigidbody2D))]
    public class FishStateManager : MonoBehaviour, ICatchable
    {
        [SerializeField] private FishMovement fishMovement;
        public FishMovement FishMovement => fishMovement;
        
        [Header("저항도(Resistance) 설정")]
        [SerializeField] private float maxResistance = 20f;
        public float MaxResistance => maxResistance;

        public float CurrentResistance { get; private set; }

        private IFishState currentState;
        private CancellationTokenSource stateCts;
        private Rigidbody2D rb;

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            fishMovement = GetComponent<FishMovement>();
        }

        private void Start()
        {
            ResetResistance();
            ChangeState(new FishIdleState());
        }

        private void Update()
        {
            currentState?.Tick();
        }

        private void OnDisable()
        {
            stateCts?.Cancel();
            stateCts?.Dispose();
            stateCts = null;
        }

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
        
        private async void TransitionTo(IFishState newState)
        {   
            if (newState == null) return;

            stateCts?.Cancel();
            stateCts?.Dispose();
            stateCts = new CancellationTokenSource();
            CancellationToken token = stateCts.Token;

            try
            {   
                if (currentState != null)
                {
                    await currentState.ExitAsync(token);
                    if (this == null) return; 
                }

                currentState = newState;
                await currentState.EnterAsync(this, token);
            }
            catch (OperationCanceledException)
            {
                Debug.Log("상태 전환 취소");
            }
        }

        #endregion
    }
}