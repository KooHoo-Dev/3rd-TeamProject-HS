using System.Threading;
using System.Threading.Tasks;

namespace WeGotOne.Fish_Interface
{
    public class FishKnockDownState : IFishState
    {
        public Task EnterAsync(FishStateManager owner, CancellationToken token)
        {
            throw new System.NotImplementedException();
        }

        public void Tick()
        {
            throw new System.NotImplementedException();
        }

        public Task ExitAsync(CancellationToken token)
        {
            throw new System.NotImplementedException();
        }
    }
}