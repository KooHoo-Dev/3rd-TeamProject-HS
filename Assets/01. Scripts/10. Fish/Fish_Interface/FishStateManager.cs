using System;
using System.Threading;
using UnityEngine;

namespace WeGotOne.Fish_Interface
{   
    [RequireComponent(typeof(Rigidbody2D))]
    
    public class FishStateManager : MonoBehaviour , ICatchable
    {

        [SerializeField] private FishMovement fishMovement;

        public FishMovement FishMovement => fishMovement;
        
        private IFishState currentState;
        private CancellationTokenSource stateCts;

        private Rigidbody2D rb;

        #region 유니티 이벤트 함수

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            fishMovement = GetComponent<FishMovement>();
        }

        private void Start()
        {
            ChangeState(new FishIdleState());
        }

        private void OnEnable()
        {
            foreach (HandControllerWithInterface hand in HandRegistry.ActiveHands)
            {
                Subscribe(hand);
            }
        }

        private void OnDisable()
        {
            foreach (HandControllerWithInterface hand in HandRegistry.ActiveHands)
            {
                Unsubscribe(hand);
            }
        }
        
        

        #endregion
        
        
        private void Subscribe(HandControllerWithInterface hand)
        {
            hand.Grabbed += target => HandleGrabbed(hand, target);
            hand.Released += () => HandleReleased(hand);
        }

        private void Unsubscribe(HandControllerWithInterface hand)
        {
            hand.Grabbed -= target => HandleGrabbed(hand, target);
            hand.Released -= () => HandleReleased(hand);
        }

        private void HandleGrabbed(HandControllerWithInterface hand, Rigidbody2D grabbedTarget)
        {
            if (grabbedTarget != rb) return; 
            OnGrabbed(hand);
        }

        private void HandleReleased(HandControllerWithInterface hand)
        {   // 이 손이 "물고기를 잡고 있던 손" 인지는 CaughtState 쪽에서 판단해야함 
            OnReleased(hand);
        }
        
        public void OnGrabbed(HandControllerWithInterface hand)
        {
            TransitionTo(new FishCaughtSate(hand));
        }

        public void OnReleased(HandControllerWithInterface hand)
        {
            TransitionTo(new FishRunawayState());
        }

        public void ChangeState(IFishState newState)
        {
            TransitionTo(newState);
        }
        
        private async void TransitionTo(IFishState newState)
        {   
            if (newState == null)
            {
                Debug.Log("전환하려는 상태가 현재 Null 이다");
                return;
            }

            
            // stateCts 가 null 이 아닐 때만 취소 
            stateCts?.Cancel();
            stateCts?.Dispose();
            stateCts = new CancellationTokenSource();
            CancellationToken token = stateCts.Token;

           
            try
            {   // 이전 상태 퇴장
                if (currentState != null)
                    await currentState.ExitAsync(token);
                // 새로운 상태 진입
                currentState = newState;
                await currentState.EnterAsync(this, token);

            }
            catch (OperationCanceledException)
            {
                //이 전환이 더 최신 전환에 의해 취소됨 
                Debug.Log("상태 전환이 취소되었다");
            }
            
        }
        
        
    }
}