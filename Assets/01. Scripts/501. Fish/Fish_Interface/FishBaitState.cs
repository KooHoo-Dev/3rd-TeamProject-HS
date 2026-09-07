using Cysharp.Threading.Tasks;
using UnityEngine;

namespace WeGotOne.Fish_Interface
{
    public class FishBaitState : IFishState
    {
        private FishStateManager owner;
        private FishStateManager targetStateManager;
        private float baitSpeed = 2.0f;
        private float waypointTolerance = 0.5f;

        public void Enter(FishStateManager owner)
        {
            Debug.Log("Start Bait");
            this.owner = owner;
        }

        public void Tick()
        {
            if (owner == null || owner.FishMovement == null) return;

            if (owner.Hunger > owner.HungerLimit) return;
            
            if(owner.BaitInfo == null)
            {
                Exit();
                return;
            }
            
            targetStateManager = owner.BaitInfo;
            
            Vector2 direction = (targetStateManager.transform.position - owner.transform.position).normalized;
            owner.FishMovement.Move(direction, baitSpeed);
            
            float distance = Vector2.Distance(owner.transform.position, targetStateManager.transform.position);
            if (distance < waypointTolerance)
            {
                FishObjectPool.Instance.Release(targetStateManager.GetComponent<WorldItem>());
                Exit();
            }
        }

        public void Exit()
        {
            targetStateManager = null;
            owner.FishMovement.StopMove();
            
            if (owner.FishName.Contains("Manta"))
            {
                Debug.Log("Attack false");
                owner.ObjectPool.Release(owner.BaitInfo.gameObject.GetComponent<WorldItem>());
                owner.ChangeState(new FishFaintState());   
                
            }
            else
                owner.ChangeState(new FishIdleState());
        }
    }
}