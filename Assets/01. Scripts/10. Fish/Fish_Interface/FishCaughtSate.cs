using System.Threading;
using System.Threading.Tasks;

namespace WeGotOne.Fish_Interface
{
    public class FishCaughtSate : IFishState
    {
        private readonly HandControllerV2 grabbingHand;

        public FishCaughtSate(HandControllerV2 hand)
        {
            grabbingHand = hand;
        }        
        
        public async Task EnterAsync(FishStateManager owner, CancellationToken token)
        {   
            // 저항도 초기화, Patrol Twenn Kill 해야함
            await Task.CompletedTask;
        }

        public void Tick()
        {
            // 추가 해야 할 것: 저항도 감소, grabbingHand 위치 추적 등..
        }

        public async Task ExitAsync(CancellationToken token)
        {
            await Task.CompletedTask;
        }
    }
}