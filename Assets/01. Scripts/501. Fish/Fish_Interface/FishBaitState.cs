using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace WeGotOne.Fish_Interface
{
    public class FishBaitState : IFishState
    {
        private FishStateManager owner;
        private FishStateManager targetStateManager;
        private float baitSpeed = 2.0f;
        private float waypointTolerance = 0.5f;

        public void EnterAsync(FishStateManager owner)
        {
            this.owner = owner;
        }

        public void Tick()
        {
            if (owner == null || owner.FishMovement == null) return;

            if (owner.Hunger > owner.HungerLimit) return;
            
            if(owner.BaitInfo == null)
            {
                ExitAsync();
                return;
            }
            
            targetStateManager = owner.BaitInfo;
            Vector2 direction = (targetStateManager.transform.position - owner.transform.position).normalized;
            owner.FishMovement.Move(direction, baitSpeed);
            
            float distance = Vector2.Distance(owner.transform.position, targetStateManager.transform.position);
            if (distance < waypointTolerance)
            {
                ExitAsync();
            }
        }

        public void ExitAsync()
        {
            targetStateManager = null;
            owner.FishMovement.StopMove();
            owner.ChangeState(new FishIdleState());
        }
    }
}