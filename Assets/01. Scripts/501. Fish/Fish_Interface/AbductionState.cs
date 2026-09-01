using UnityEngine;
using UnityEngine.LowLevelPhysics2D;

namespace WeGotOne.Fish_Interface
{
    public class AbductionState : IFishState
    {
        private FishStateManager owner;
        private float moveSpeed = 5.0f; // 끌고 갈 때는 조금 더 빠르게

        private Transform caughtPlayer;
        private bool isFloorHit = false;
        
        private Transform targetPosition;
        private float destinationTolerance = 0.5f;

        public void EnterAsync(FishStateManager owner)
        {
            this.owner = owner;
            caughtPlayer = owner.PlayerRootPosition; // 매니저에 저장된 플레이어 정보 가져오기
            isFloorHit = false;
        }

        public void Tick()
        {
            if (isFloorHit) return;

            // 매니저(owner)가 가지고 있는 목적지 정보를 참조하여 방향 계산
            Vector2 direction = (owner.AbductionTarget.position - owner.transform.position).normalized;
    
            owner.FishMovement.Move(direction, moveSpeed);
    
            // 플레이어 위치 강제 고정
            if (caughtPlayer != null)
            {
                caughtPlayer.position = owner.transform.position; 
            }
        }

        // 바닥 센서가 닿았을 때 매니저를 통해 호출됨
        public void HandleFloorHit()
        {
            isFloorHit = true;
            ExitAsync();

            owner.ChangeState(new FishPatrolState());
        }

        public void ExitAsync()
        {
            if (owner != null && owner.FishMovement != null)
            {
                // 이동 정지[cite: 6]
                owner.FishMovement.StopMove();
            }

            // 플레이어 놓아주기 (조작 복구 로직 포함 필요)
            caughtPlayer = null;
        }
    }
}