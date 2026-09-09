using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace WeGotOne.Fish_Interface
{
    public class FishCaughtState : IFishState
    {
        private FishStateManager owner;
        private FishCollisionHandler collisionHandler;
        private float collisionRatio = 0.2f;

        private Vector2 struggleDirection;
        private float struggleForceMultiplier = 10f;
        private float directionChangeTimer;

        public void Enter(FishStateManager owner)
        {
            this.owner = owner;
            
            Debug.Log($"{owner.name} : 힘겨루기 상태(Caught) 진입.");

            SetNewStruggleDirection();

            // 충돌 이벤트 구독
            collisionHandler = owner.GetComponent<FishCollisionHandler>();
            if (collisionHandler != null)
            {
                collisionHandler.OnTerrainSmashed += HandleTerrainSmashed;
            }
        }

        public void Tick()
        {
            if (owner == null) return;

            // 1. 발버둥 방향 전환 타이머
            directionChangeTimer -= Time.deltaTime;
            if (directionChangeTimer <= 0f)
            {
                SetNewStruggleDirection();
            }

            // 2. 저항도 감소 및 비율 계산
            owner.DecreaseResistance(Time.deltaTime);
            float ratio = owner.CurrentResistance / owner.MaxResistance;

            // 3. 남은 저항도 비율에 맞춰 물리적인 반발력(Struggle Force)을 계산해 명령
            Vector2 appliedForce = struggleDirection * (struggleForceMultiplier * ratio);
            owner.FishMovement.ApplyForce(appliedForce);

            // 4. 탈진 체크
            if (owner.CurrentResistance <= 0f)
            {
                if (collisionHandler != null)
                {
                    collisionHandler.OnTerrainSmashed -= HandleTerrainSmashed;
                }
                
                owner.ChangeState(new FishExhaustedState());
            }
        }

        private void SetNewStruggleDirection()
        {
            // 0.5초 ~ 1.5초마다 무작위 방향으로 발버둥
            struggleDirection = Random.insideUnitCircle.normalized;
            directionChangeTimer = Random.Range(0.5f, 1.5f);
        }

        private void HandleTerrainSmashed(float impactStrength, float sfxPitch)
        {
            if (owner == null) return;

            if(owner.HitParticle)
                owner.HitParticle.Play();
            if(AudioManager.Instance.sfxClips[(int)AudioManager.Sfx.FishTerrainSmashed])
                AudioManager.Instance.PlaySfx(AudioManager.Sfx.FishTerrainSmashed, sfxPitch);
            float damageToResistance = impactStrength * collisionRatio;
            owner.DecreaseResistance(damageToResistance);
            Debug.Log($"지형 충돌! 저항도 {owner.name} :{damageToResistance:F1} 감소.");
        }
        
        public void Exit()
        {   
            if (collisionHandler != null)
            {
                collisionHandler.OnTerrainSmashed -= HandleTerrainSmashed;
            }
            if (owner != null && owner.FishMovement != null)
            {
                owner.FishMovement.StopMove();
            }
        }
    }
}