using UnityEngine;
using WeGotOne.Fish_Interface;

namespace WeGotOne
{
    public class FishOctopusState : IFishState
    {
        /*
         * 문어 :
         * 감지 거리 내에 플레이어가 감지되면
         * 먹물을 한번 쏘고 도망다님.
         * 이후 플레이어가 감지거리를 벗어나기 전까진 도망만 다니고,
         * 상태가 초기화되는걸 기점으로 발사체를 쏠 수 있는 횟수가 초기화 됨
         */

        private Transform target;
        private FishStateManager owner;

        private float runawaySpeed = 6.0f;
        private float runawayDuration = 5.0f;
        private float runawayTimer;

        private float bulletSpeed = 5f;

        private bool hasShot;
        
        private GameObject bulletInstance;

        public FishOctopusState(Transform target)
        {
            this.target = target;
        }

        public void Enter(FishStateManager owner)
        {
            this.owner = owner;

            if (target == null)
            {
                owner.ChangeState(new FishIdleState());
                return;
            }

            hasShot = false;

            ShootBullet();
        }

        public void Tick()
        {
            if (target == null)
            {
                owner.ChangeState(new FishIdleState());
                return;
            }

            if (hasShot == false)
            {
                ShootBullet();
            }

            RunAway();
        }

        public void Exit()
        {
        }

        private void ShootBullet()
        {
            if (hasShot)
                return;
            
            if (owner == null)
                return;
            
            if (owner.BulletPrefab == null)
            {
                owner.ChangeState(new FishIdleState());
            }
            
            if (bulletInstance == null)
            {
                bulletInstance = Object.Instantiate(owner.BulletPrefab);

                bulletInstance.SetActive(false);
            }
            
            bulletInstance.transform.position = owner.transform.position;
            bulletInstance.transform.rotation = Quaternion.identity;

            Vector2 direction = (target.transform.position - owner.transform.position).normalized;

            bulletInstance.SetActive(true);
            
            if (bulletInstance.TryGetComponent(out Bullet bulletScript))
            {
                bulletScript.Fire(direction, bulletSpeed);
            }

            hasShot = true;
        }

        private void RunAway()
        {
            if (owner == null || owner.FishMovement == null)
                return;        
            
            Vector2 runawayDirection = (owner.transform.position - target.transform.position).normalized;

            owner.FishMovement.Move(runawayDirection, runawaySpeed);

            runawayTimer += Time.deltaTime;

            if (runawayTimer >= runawayDuration)
            {
                owner.ChangeState(new FishIdleState());
            }
        }
    }
}