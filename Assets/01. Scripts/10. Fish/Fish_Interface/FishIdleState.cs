using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace WeGotOne.Fish_Interface
{
    
    public class FishIdleState : IFishState
    {
        private readonly float idleDruation;
        private FishStateManager owner;

        public FishIdleState(float idleDruation = 1.5f)
        {
            this.idleDruation = idleDruation;
        }
        
        public async Task EnterAsync(FishStateManager owner, CancellationToken token)
        {
            Debug.Log($"[IdleState] : Idle 상태 진입");
            this.owner = owner;

            await Task.Delay(TimeSpan.FromSeconds(idleDruation), token);
            
            token.ThrowIfCancellationRequested();
            owner.ChangeState(new FishPatrolState(owner.FishMovement));
        }

        public void Tick()
        {
            
        }

        public Task ExitAsync(CancellationToken token)
        {
            return Task.CompletedTask;
        }
    }
}