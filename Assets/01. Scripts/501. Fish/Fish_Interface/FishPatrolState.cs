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

        public void EnterAsync(FishStateManager owner)
        {
            this.owner = owner;
            SetNewWaypoint(); // 첫 목적지 설정
        }

        public void Tick()
        {
            if (owner == null || owner.FishMovement == null) return;

            // 1. 목적지에 도착했는지 판단
            float distance = Vector2.Distance(owner.transform.position, targetPosition);
            if (distance < waypointTolerance)
            {
                SetNewWaypoint();
            }

            // 2. 가야 할 방향 계산 후 이동 명령
            Vector2 direction = (targetPosition - (Vector2)owner.transform.position).normalized;
            owner.FishMovement.Move(direction, patrolSpeed);
        }

        public void ExitAsync()
        {
            if (owner != null && owner.FishMovement != null)
            {
                owner.FishMovement.StopMove();
            }
        }

        public void SetNewWaypoint()
        {
            if (owner == null)
                return;

            Collider2D waterArea = owner.FishMovement.WaterArea;

            if (waterArea == null)
                return;

            // fish 종류에 따라 분기처리 (FishStateManager의 fishName이 설정되어 있지 않다면 기본 웨이포인트 설정 (수면 아래 랜덤한 위치로 설정)
            switch (owner.fishName)
            {
                case "MineFish":
                    // 해보니까 더 천천히 움직여야함
                    // 그리고 일정시간마다 지가 알아서 방향 틀어야
                default:
                    GenerateRandomWaypoint(waterArea, out var maxTryCount);
                    break;
            }
        }

        private void GenerateRandomWaypoint(Collider2D waterArea, out int maxTryCount)
        {
            maxTryCount = 30;

            for (int i = 0; i < maxTryCount; i++)
            {
                Vector2 randomCircle = Random.insideUnitCircle * 5f;

                Vector2 candidatePosition = (Vector2)owner.transform.position + randomCircle;

                // 후보 위치가 물 안에 있을 때만 목표로 설정
                if (waterArea.OverlapPoint(candidatePosition))
                {
                    targetPosition = candidatePosition;
                    owner.TargetPosition = candidatePosition;
                    return;
                }
            }

            // 적당한 목표를 못 찾으면 현재 위치 유지
            targetPosition = owner.transform.position;
            owner.TargetPosition = targetPosition;
        }
    }
}