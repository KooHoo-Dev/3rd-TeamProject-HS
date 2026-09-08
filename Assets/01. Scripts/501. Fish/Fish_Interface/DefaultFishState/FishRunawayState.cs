using System;
using System.Collections;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using Object = UnityEngine.Object;
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

        public void Enter(FishStateManager owner)
        {
            this.owner = owner;
            Debug.Log($"{owner.name} : 도망 상태(Runaway) 시작.");

            // 도망칠 방향 계산 (예시: 플레이어의 반대 방향이나 랜덤 방향)
            runawayDirection = Random.insideUnitCircle.normalized;
            runawayTimer = 0;
            // AudioManager.Instance.PlaySfx(AudioManager.Sfx.FishRunaway);
            if (owner.RunawayParticle)
                owner.RunawayParticle.SetActive(true);
            if (owner.Animator)
                owner.Animator.speed = 20;
        }

        public void Tick()
        {
            if (owner == null || owner.FishMovement == null) return;

            // 매 프레임 모터(Movement)에 도망 지시
            owner.FishMovement.Move(runawayDirection, runawaySpeed);

            runawayTimer += Time.deltaTime;

            if (runawayTimer >= runawayDuration)
            {
                if (owner.Animator != null)
                {
                    owner.Animator.speed = 1;
                }

                if (owner.RunawayParticle)
                    owner.RunawayParticle.SetActive(false);
                owner.ChangeState(new FishIdleState());
            }
        }

        public void Exit()
        {
            if (owner != null && owner.FishMovement != null)
            {
                if (owner.Animator != null)
                {
                    owner.Animator.speed = 1;
                }

                if (owner.RunawayParticle)
                    owner.RunawayParticle.SetActive(false);
                owner.FishMovement.StopMove();
            }
        }
    }
}