using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using Random = UnityEngine.Random;

namespace WeGotOne.Fish_Interface
{
    public class FishRunawayState : IFishState
    {
        private FishStateManager owner;
        private Vector2 runawayDirection;
        private float runawaySpeed = 6.0f;
        private float runawayDuration = 3.0f;
        
        private float runawayTimer;

        public void EnterAsync(FishStateManager owner)
        {
            this.owner = owner;
            Debug.Log($"{owner.name} : 도망 상태(Runaway) 시작.");

            // 도망칠 방향 계산 (예시: 플레이어의 반대 방향이나 랜덤 방향)
            runawayDirection = Random.insideUnitCircle.normalized;

            runawayTimer = 0;
        }

        public void Tick()
        {
            if (owner == null || owner.FishMovement == null) return;
            
            // 매 프레임 모터(Movement)에 도망 지시
            owner.FishMovement.Move(runawayDirection, runawaySpeed);
            
            runawayTimer += Time.deltaTime;
            
            if (runawayTimer >= runawayDuration)
            {
                owner.ChangeState(new FishIdleState());
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