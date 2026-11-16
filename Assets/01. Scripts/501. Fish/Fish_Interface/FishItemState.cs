using UnityEngine;

namespace WeGotOne.Fish_Interface
{
    public class FishItemState : IFishState
    {
        private FishStateManager owner;
        Rigidbody2D playerRigidbody;
        
        public void Enter(FishStateManager owner)
        {
            this.owner = owner;
        }

        public void Tick()
        {
        }

        public void Exit()
        {
        }
    }
}