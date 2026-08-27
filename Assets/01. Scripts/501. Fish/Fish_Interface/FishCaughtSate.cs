using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace WeGotOne.Fish_Interface
{
    public class FishCaughtState : IFishState
    {
        private FishStateManager owner;
        private FishCollisionHandler collisionHandler;

        private float collisionRatio = 0.2f; // 충격량 값 계산비

        public Task EnterAsync(FishStateManager owner, CancellationToken token)
        {
            this.owner = owner;
            Debug.Log($"{owner.name} : 힘겨루기 상태(Caught) 진입.");

            // 1. [진입 시점]: 발버둥 타이머 및 첫 도망 방향을 초기화합니다.
            owner.FishMovement.InitStruggle();
            
            // 2. 충돌 컴포넌트 이벤트 구독 
            collisionHandler = owner.GetComponent<FishCollisionHandler>();
            if (collisionHandler != null)
            {
                collisionHandler.OnTerrainSmashed += HandleTerrainSmashed;
            }

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
                owner.ChangeState(new FishExhaustedState());
            }
        }

        private void HandleTerrainSmashed(float impactStrength)
        {
            if (owner == null) return;
            
            // 충격이 강할 수록 더 많은 저항도를 즉각 소진
            // 계수(0.5f) 는 플레이 테스트를 거치며 밸런싱 조절 하면 됨

            float damageToResistance = impactStrength * collisionRatio; 
            owner.DecreaseResistance(damageToResistance);
            
            Debug.Log($"<color=orange>[지형 충격]</color> <b>{owner.name}</b> 지형 충돌! " + 
                      $"저항도 <b>{damageToResistance:F1}</b> 급감! (남은 저항도: {owner.CurrentResistance:F1})");
        }
        
        
        
        
        public Task ExitAsync(CancellationToken token)
        {   
            
            // / 3. [퇴장 시점]: 힘겨루기 상태를 벗어날 때, 이벤트 안전 해제와
            // 잔여 물리 속도와 힘을 안전하게 정지시킵니다.

            if (collisionHandler != null)
            {
                collisionHandler.OnTerrainSmashed -= HandleTerrainSmashed;
                collisionHandler = null;
                
            }
            
            if (owner != null)
            {
                
                owner.FishMovement.StopMove();
            }
            return Task.CompletedTask;
        }
    }
}