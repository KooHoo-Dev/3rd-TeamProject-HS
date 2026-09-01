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

        public async Task EnterAsync(FishStateManager owner, CancellationToken token)
        {
            this.owner = owner;
            Debug.Log($"{owner.name} : 도망 상태(Runaway) 시작.");

            // 도망칠 방향 계산 (예시: 플레이어의 반대 방향이나 랜덤 방향)
            runawayDirection = Random.insideUnitCircle.normalized;

            try
            {
                // 비동기 대기로 도망 유지 시간 제어
                await Task.Delay(TimeSpan.FromSeconds(runawayDuration), token);

                if (owner == null || token.IsCancellationRequested) return;

                // 무사히 도망쳤다면 Idle 상태로 복귀
                owner.ChangeState(new FishIdleState());
            }
            catch (OperationCanceledException)
            {
                Debug.Log($"{owner.name} : 도망 상태 취소됨.");
            }
        }

        public void Tick()
        {
            if (owner == null || owner.FishMovement == null) return;
            
            // 매 프레임 모터(Movement)에 도망 지시
            owner.FishMovement.Move(runawayDirection, runawaySpeed);
        }

        public Task ExitAsync(CancellationToken token)
        {
            if (owner != null && owner.FishMovement != null)
            {
                owner.FishMovement.StopMove();
            }
            return Task.CompletedTask;
        }
    }
}