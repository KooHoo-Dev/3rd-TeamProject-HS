using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace WeGotOne.Fish_Interface
{
    /// <summary>
    /// 플레이어의 손에 완전히 힘없이 늘어진 탈진 상태입니다.
    /// 잡혀있는 동안에는 아무런 저항을 하지 않고 유지됩니다.
    /// </summary>
    public class FishExhaustedState : IFishState
    {
        private readonly HandControllerV2 grabHand;

        public FishExhaustedState(HandControllerV2 hand)
        {
            this.grabHand = hand;
        }

        public Task EnterAsync(FishStateManager owner, CancellationToken token)
        {
            Debug.Log($"{owner.name} : 완전히 탈진했습니다. 손쉽게 낚아올릴 수 있는 기회입니다!");
            
            // 저항을 멈추고 얌전하게 힘을 뺍니다.
            owner.FishMovement.StopMove(); 
            return Task.CompletedTask;
        }

        public void Tick()
        {
            // 탈진했으므로 아무런 매 프레임 업데이트 행동을 하지 않습니다.
        }

        public Task ExitAsync(CancellationToken token)
        {   
            
            return Task.CompletedTask;
        }
    }
}