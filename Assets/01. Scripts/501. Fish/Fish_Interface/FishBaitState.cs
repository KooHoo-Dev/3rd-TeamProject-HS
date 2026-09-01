using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace WeGotOne.Fish_Interface
{
    public class FishBaitState : IFishState
    {
        private FishStateManager owner;
        private Vector2 targetPosition;
        private float baitSpeed = 2.0f;
        private float waypointTolerance = 0.5f;

        public void EnterAsync(FishStateManager owner)
        {
            this.owner = owner;
            targetPosition = owner.BaitPosition;
        }

        public void Tick()
        {
            if (owner == null || owner.FishMovement == null) return;
            
            Vector2 direction = (targetPosition - (Vector2)owner.transform.position).normalized;
            owner.FishMovement.Move(direction, baitSpeed);
            
            float distance = Vector2.Distance(owner.transform.position, targetPosition);
            if (distance < waypointTolerance)
            {
                targetPosition = new Vector2(0,0);
                Debug.Log("도착");
            }
        }

        public void ExitAsync()
        {
            if (owner != null && owner.FishMovement != null)
            {
                owner.FishMovement.StopMove();
            }
        }
    }
}