using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace WeGotOne.Fish_Interface
{
    /// <summary>
    /// 플레이어가 손을 놓았을 때 저항도가 낮아(30% 미만) 일시적으로 물 위에서 기절해 있는 상태입니다.
    /// </summary>
    public class FishFaintState : IFishState
    {
        public async Task EnterAsync(FishStateManager owner, CancellationToken token)
        {
            Debug.Log($"{owner.name} : 물 위에 떠 있는 기절(Faint) 상태 돌입.");
            owner.FishMovement.StopMove();

            // 최대 저항도가 높은 물고기일수록 회복력이 뛰어나 빠르게 일어남
            float baseFaintTime = 6.0f;
            float recoveryOffset = owner.MaxResistance * 0.15f; 
            float finalFaintDuration = Mathf.Max(1.5f, baseFaintTime - recoveryOffset);

            Debug.Log($"{owner.name} : {finalFaintDuration:F1}초 동안 무방비하게 둥둥 뜹니다.");

            try
            {
                await Task.Delay(TimeSpan.FromSeconds(finalFaintDuration), token);

                if (owner == null || token.IsCancellationRequested) return;

                // 회복 시 원래의 최대 체력으로 복구하고 다시 평화롭게 수영
                owner.ResetResistance();
                owner.ChangeState(new FishIdleState());
            }
            catch (OperationCanceledException)
            {
                Debug.Log($"{owner.name} : 기절 상태가 취소되었습니다 (재포획 등).");
            }
        }

        public void Tick()
        {
        }

        public Task ExitAsync(CancellationToken token)
        {
            return Task.CompletedTask;
        }
    }
}