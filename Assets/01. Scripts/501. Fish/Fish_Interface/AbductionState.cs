using System.Collections.Generic;
using UnityEngine;

namespace WeGotOne.Fish_Interface
{
    public class AbductionState : IFishState
    {
        private FishStateManager owner;
        
        private float moveSpeed = 10.0f; // 끌고 갈 때는 조금 더 빠르게

        private Transform caughtPlayer;
        private Rigidbody2D[] playerRigidbodies;

        private Collider2D[] playerColliders;
        private Collider2D fishCollider;
        
        private Vector3 interval = new(3.5f, 6f);
        
        private Transform targetPosition;
        private float destinationTolerance = 1.0f;

        public void Enter(FishStateManager owner)
        {
            this.owner = owner;
            
            caughtPlayer = owner.PlayerRootPosition.GetChild(0);
            
            fishCollider = owner.GetComponent<Collider2D>();
            
            playerColliders = owner.PlayerRootPosition.GetComponentsInChildren<Collider2D>();
            
            if (fishCollider != null)
            {
                foreach (var col in playerColliders)
                {
                    Physics2D.IgnoreCollision(fishCollider, col, true);
                }
            }
            
            ChangeToKinematic();
        }

        private void ChangeToKinematic()
        {
            // 1. 하위 계층(모든 자식/손자)의 Rigidbody2D를 통째로 가져옴
            playerRigidbodies = owner.PlayerRootPosition.GetComponentsInChildren<Rigidbody2D>();
            BottomArmController[] playerBottomArms =
                owner.PlayerRootPosition.GetComponentsInChildren<BottomArmController>();

            // 2. 전체 순회하며 Kinematic 전환
            foreach (var rb in playerRigidbodies)
            {
                rb.linearVelocity = Vector2.zero;
                rb.bodyType = RigidbodyType2D.Kinematic;
            }
            foreach (var ba in playerBottomArms)
            {
                ba.grabJoint.connectedBody = null;
            }
        }

        public void Tick()
        {
            // 매니저(owner)가 가지고 있는 목적지 정보를 참조하여 방향 계산
            // 나중에 스폰 매니저 결정되면 스폰매니저가 납치 장소 주입해주기
            // 지금은 인게임씬 활성화 용으로 납치 좌표 하드코딩

            Vector2 destination = owner.AbductionDestination.position - owner.transform.position;
            float distance = Vector2.Distance(owner.transform.position, owner.AbductionDestination.position);
            Debug.Log(owner.AbductionDestination.name);
            
            owner.FishMovement.Move(destination, moveSpeed);
    
            // 플레이어 위치 강제 고정
            if (caughtPlayer != null)
            {
                caughtPlayer.position = owner.PlayerAttachPosition.position;
            }
            
            Debug.Log(distance);
            if (distance < destinationTolerance)
            {
                Exit();
            }
        }

        public void Exit()
        {
            if (owner != null && owner.FishMovement != null)
            {
                owner.FishMovement.StopMove();
            }

            #region Ai추가

            PlayerController player = owner.PlayerRootPosition.GetComponentInParent<PlayerController>();
    
            // 2. 플레이어가 Start() 때 스스로 저장해 둔 초기 포즈를 꺼내 복원 함수 실행
            if (player != null && player.InitialPlayerState != null)
            {
                player.RestoreToInitialPose(player.InitialPlayerState);
            }
            
            if (fishCollider != null && playerColliders != null)
            {
                foreach (var col in playerColliders)
                {
                    if (col != null)
                    {
                        Physics2D.IgnoreCollision(fishCollider, col, false);
                    }
                }
            }

            #endregion

            // 6. 모든 신체 부위를 다시 Dynamic으로 정상 복원
            if (playerRigidbodies != null)
            {
                foreach (var rb in playerRigidbodies)
                {
                    if (rb != null)
                    {
                        rb.bodyType = RigidbodyType2D.Dynamic;
                        rb.linearVelocity = Vector2.zero;
                    }
                }
            }
            
            Debug.Log("Exit");
            caughtPlayer = null;
            owner.ChangeState(new FishPatrolState());
        }
    }
}