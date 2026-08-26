using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace WeGotOne.Fish_Interface
{
    public class FishCaughtState : IFishState
    {
        private readonly HandControllerV2 grabHand;
        private FishStateManager owner;

        public FishCaughtState(HandControllerV2 hand)
        {
            this.grabHand = hand;
        }

        public Task EnterAsync(FishStateManager owner, CancellationToken token)
        {
            this.owner = owner;
            Debug.Log($"{owner.name} : 힘겨루기 상태(Caught) 진입. 대각선 회피 AI 기반 물리 몸부림을 시작합니다.");

            // 1. [진입 시점]: 발버둥 타이머 및 첫 도망 방향(GetFleeDirection)을 초기화합니다.
            owner.FishMovement.InitStruggle();

            return Task.CompletedTask;
        }

        public void Tick()
        {
            if (owner == null) return;

            // 1. 저항도 깎기 (매 프레임 감소)
            owner.DecreaseResistance(Time.deltaTime);
            float ratio = owner.CurrentResistance / owner.MaxResistance;

            // 2. [물리 연출 위임]: Runaway와 공유하는 도망 방향 알고리즘으로 Rigidbody2D에 힘을 가합니다.
            owner.FishMovement.ApplyStruggleForce(ratio);

            // 3. 탈진 체크 (저항도가 0이 되면 잡힌 채 힘을 잃는 샌드백 상태로 전환)
            if (owner.CurrentResistance <= 0f)
            {
                owner.ChangeState(new FishExhaustedState(grabHand));
            }
        }

        public Task ExitAsync(CancellationToken token)
        {
            if (owner != null)
            {
                // 3. [퇴장 시점]: 힘겨루기 상태를 벗어날 때, 잔여 물리 속도와 힘을 안전하게 정지시킵니다.
                owner.FishMovement.StopMove();
            }
            return Task.CompletedTask;
        }
    }
}