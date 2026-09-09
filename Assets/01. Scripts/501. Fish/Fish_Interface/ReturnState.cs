using UnityEngine;

namespace WeGotOne.Fish_Interface
{
    public class ReturnState : IFishState
    {
        private FishStateManager owner;
        private Vector2 startPosition;
        private float returnSpeed = 3.0f;

        public void Enter(FishStateManager owner)
        {
            this.owner = owner;
            startPosition = owner.StartPosition;
            Debug.Log("returning");
        }

        public void Tick()
        {
            owner.FishMovement.Move(startPosition,returnSpeed);

            if (Vector2.Distance(startPosition, owner.transform.position) < 1.0f)
            {
                Exit();
            }
        }

        public void Exit()
        {
            owner.FishMovement.StopMove();
            owner.ChangeState(new FishPatrolState());
        }
    }
}