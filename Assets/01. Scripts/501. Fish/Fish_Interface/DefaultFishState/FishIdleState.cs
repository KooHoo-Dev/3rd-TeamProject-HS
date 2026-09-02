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
        private float idleTimer;

        public FishIdleState(float idleDruation = 1.5f)
        {
            this.idleDruation = idleDruation;
        }
        
        public void Enter(FishStateManager owner)
        {
            this.owner = owner;
        }

        public void Tick()
        {
            if (owner == null || owner.FishMovement == null) return;
            
            idleTimer += Time.deltaTime;
            
            if (idleTimer >= idleDruation)
            {
                owner.ChangeState(new FishPatrolState());
            }
        }

        public void Exit()
        {
        }
    }
}