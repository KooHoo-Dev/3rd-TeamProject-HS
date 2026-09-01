using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace WeGotOne.Fish_Interface
{
    public class FishPatrolState : IFishState
    {
        private FishStateManager owner;
        private Vector2 targetPosition;
        private float patrolSpeed = 2.0f;
        private float waypointTolerance = 0.5f;

        public Task EnterAsync(FishStateManager owner, CancellationToken token)
        {
            this.owner = owner;
            SetNewWaypoint(); // 첫 목적지 설정
            return Task.CompletedTask;
        }

        public void Tick()
        {
            if (owner == null || owner.FishMovement == null) return;

            // 1. 목적지에 도착했는지 판단
            float distance = Vector2.Distance(owner.transform.position, targetPosition);
            if (distance < waypointTolerance)
            {
                owner.TargetPosition = targetPosition;
                SetNewWaypoint();
            }

            // 2. 가야 할 방향 계산 후 이동 명령
            Vector2 direction = (targetPosition - (Vector2)owner.transform.position).normalized;
            owner.FishMovement.Move(direction, patrolSpeed);
        }

        public Task ExitAsync(CancellationToken token)
        {
            if (owner != null && owner.FishMovement != null)
            {
                owner.FishMovement.StopMove();
            }
            return Task.CompletedTask;
        }

        private void SetNewWaypoint()
        {
            // 랜덤 반경 안에서 새로운 목적지 설정
            Vector2 randomCircle = Random.insideUnitCircle * 5f;
            targetPosition = (Vector2)owner.transform.position + randomCircle;
        }
    }
}