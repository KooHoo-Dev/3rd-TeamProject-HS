using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace WeGotOne.Fish_Interface
{
    public class FishPatrolState : IFishState
    {
        private readonly FishMovement fishMovement;

        public FishPatrolState(FishMovement fishMovement)
        {
            this.fishMovement = fishMovement;
        }
        
        public Task EnterAsync(FishStateManager owner, CancellationToken token)
        {
            if (fishMovement == null) return Task.CompletedTask;
            Debug.Log($"[PatrolState] : Patrol 상태 진입");
            fishMovement.PullingInit(fishMovement.transform.position);
            return Task.CompletedTask;
        }

        public void Tick()
        {
           
        }

        public Task ExitAsync(CancellationToken token)
        {
            if(fishMovement != null) fishMovement.StopMove();
            return Task.CompletedTask;
        }
    }
}