using UnityEngine;

namespace WeGotOne.Fish_Interface
{
    public class AbductionState : IFishState
    {
        private FishStateManager owner;
        
        private float moveSpeed = 5.0f; // 끌고 갈 때는 조금 더 빠르게

        private Transform caughtPlayer;
        private Rigidbody2D playerRig;
        
        private Vector3 interval = new Vector3(3.5f, 6f);
        private float floorDistance = 0.5f;
        
        private Transform targetPosition;
        private float destinationTolerance = 0.5f;

        public void EnterAsync(FishStateManager owner)
        {
            this.owner = owner;
            caughtPlayer = owner.PlayerRootPosition.GetChild(0).GetComponent<Transform>(); // 매니저에 저장된 플레이어 정보 가져오기
            playerRig = caughtPlayer.GetComponent<Rigidbody2D>();
            playerRig.bodyType = RigidbodyType2D.Kinematic;
        }

        public void Tick()
        {
            // 매니저(owner)가 가지고 있는 목적지 정보를 참조하여 방향 계산
            // 나중에 스폰 매니저 결정되면 스폰매니저가 납치 장소 주입해주기
            // 지금은 인게임씬 활성화 용으로 납치 좌표 하드코딩
            Vector2 hardCodedAbductionTarget = new Vector2(150, -45);
            Vector2 direction = (hardCodedAbductionTarget - (Vector2) owner.transform.position);
    
            owner.FishMovement.Move(direction, moveSpeed);
    
            // 플레이어 위치 강제 고정
            if (caughtPlayer != null)
            {
                caughtPlayer.position = owner.transform.position - interval;
            }

            if (direction.sqrMagnitude <= (floorDistance * floorDistance)) // 이거 이미 동일해서 무한 반복
            {

                ExitAsync();
            }
        }

        public void ExitAsync()
        {
            if (owner != null && owner.FishMovement != null)
            {
                owner.FishMovement.StopMove();
            }
            
            playerRig.bodyType = RigidbodyType2D.Dynamic;
            caughtPlayer = null;
            owner.ChangeState(new FishPatrolState());
        }
    }
}