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
            Debug.Log($"{owner.name} : 힘겨루기 상태(Caught) 진입.");

            // [개선]: 움직임 스크립트에게 시각 떨림 효과를 시작해달라고 지시
            owner.FishMovement.StartCaughtShake();

            return Task.CompletedTask;
        }

        public void Tick()
        {
            if (owner == null) return;

            // 1. 저항도 깎기
            owner.DecreaseResistance(Time.deltaTime);
            float ratio = owner.CurrentResistance / owner.MaxResistance;

            // 2. [개선]: 물리 및 시각 움직임의 세부 수치 업데이트를 전적으로 위임
            owner.FishMovement.ApplyStruggleForce(ratio);
            owner.FishMovement.UpdateCaughtShake(ratio);

            // 3. 탈진 체크
            if (owner.CurrentResistance <= 0f)
            {
                owner.ChangeState(new FishExhaustedState(grabHand));
            }
        }

        public Task ExitAsync(CancellationToken token)
        {
            if (owner != null)
            {
                // [개선]: 상태 퇴장 시 떨림 끄기 호출
                owner.FishMovement.StopCaughtShake();
            }
            return Task.CompletedTask;
        }
    }
}