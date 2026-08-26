using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace WeGotOne.Fish_Interface
{
    public class FishRunawayState : IFishState
    {
        public async Task EnterAsync(FishStateManager owner, CancellationToken token)
        {
            Debug.Log($"{owner.name} : 앗! 플레이어가 스쳐 지나갔습니다. 도망 상태(Runaway)를 시작합니다.");

            try
            {
                // 1. 물고기 움직임 컴포넌트에 위임하여 빠른 탈출 질주 비동기 대기
                await owner.FishMovement.RunawayMoveAsync(token);

                // 2. 비동기 대기 도중 컴포넌트가 파괴되거나 꺼지지 않았는지 검사 (Unity 생명주기 안전망)
                if (owner == null || token.IsCancellationRequested) return;

                // 3. 도망 연출이 무사히 끝났다면 다시 원래의 차분한 대기 상태(Idle)로 자동 전이
                owner.ChangeState(new FishIdleState());
            }
            catch (OperationCanceledException)
            {
                Debug.Log($"{owner.name} : 도망 도중 상태 전환이 취소되었습니다.");
            }
        }

        public void Tick()
        {
            // 모든 움직임과 갱신 제어는 EnterAsync의 RunawayMoveAsync 테스크 내에서 흐름을 처리하므로, 
            // 매 프레임 실행되는 Tick에서는 추가 연산을 비워두어 CPU 낭비를 아낍니다.
        }

        public Task ExitAsync(CancellationToken token)
        {
            Debug.Log("도망 상태(Runaway)를 안전하게 종료하고 빠져나갑니다.");
            return Task.CompletedTask;
        }
    }
}