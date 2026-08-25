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
        
        private IFishState currentState;
        private CancellationTokenSource stateCts;

        private void Awake()
        {
            fishMovement = GetComponent<FishMovement>();
        }

        private void Start()
        {
            ChangeState(new FishIdleState());
        }

        private void OnDisable()
        {
            // 진행 중인 비동기 상태 전환만 안전하게 취소
            stateCts?.Cancel();
            stateCts?.Dispose();
            stateCts = null;
        }

        #region ICatchable 인터페이스 구현 (손이 직접 호출하는 창구)

        public void OnGrabbed(HandControllerV2 hand)
        {
            TransitionTo(new FishCaughtSate(hand));
        }

        public void OnReleased(HandControllerV2 hand)
        {
            TransitionTo(new FishRunawayState());
        }

        public void OnMissed()
        {
            if (currentState is FishIdleState)
            {
                ChangeState(new FishRunawayState());
            }
        }

        #endregion

        #region 비동기 상태 머신 코어 (FSM)

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