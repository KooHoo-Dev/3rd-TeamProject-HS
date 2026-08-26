using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace WeGotOne.Fish_Interface
{
    public class FishFaintState : IFishState
    {
        public async Task EnterAsync(FishStateManager owner, CancellationToken token)
        {
            // 관성 드래프트 시간 설정 (예: 1초 동안 미끄러짐)
            float driftDuration = 3.0f;
            
            Debug.Log($"{owner.name} : 기절(Faint) 상태 진입. {driftDuration}초간 관성 미끄러짐 시작.");
            
            // [개선]: 물리 반동값을 살려 미끄러지듯 정지하도록 전적으로 위임
            owner.FishMovement.StartFaintDrift(driftDuration);

            // 기절 지속 시간 계산 (기존 기획 공식 유지)
            float baseFaintTime = 6.0f;
            float recoveryOffset = owner.MaxResistance * 0.15f; 
            float finalFaintDuration = Mathf.Max(1.5f, baseFaintTime - recoveryOffset);

            try
            {
                // 기절이 풀릴 때까지 전체 시간 대기
                await Task.Delay(TimeSpan.FromSeconds(finalFaintDuration), token);

                if (owner == null || token.IsCancellationRequested) return;

                // 정신 차리고 회복
                owner.ResetResistance();
                owner.ChangeState(new FishIdleState());
            }
            catch (OperationCanceledException)
            {
                Debug.Log($"{owner.name} : 기절 상태가 취소되었습니다.");
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