using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace WeGotOne.Fish_Interface
{
    public class Shark : IFishState
    {
        private enum TargetType
        {
            None,
            Player,
            Fish
        }

        private FishStateManager owner;
        private FishCollisionHandler collisionHandler;
        private Collider2D target;
        private FishStateManager fish;
        private PlayerController player;
        private float collisionRatio = 0.2f;
        private float idleTimer = 3f;
        private float rushSpeed = 5f;
        private Vector3 rushDirection;

        private int randNum;

        public Shark(Collider2D target)
        {
            this.target = target;
        }

        public void Enter(FishStateManager owner)
        {
            this.owner = owner;
            
            collisionHandler = owner.GetComponent<FishCollisionHandler>();

            if (target.TryGetComponent(out FishStateManager fish) == true)
            {
                this.fish = fish;
            }
            else
            {
                this.fish = null;
            }
            
            player = target.GetComponentInParent<PlayerController>();
            rushDirection = target.transform.position - owner.transform.position;

            if (collisionHandler != null)
            {
                collisionHandler.OnTerrainSmashed += HandleTerrainSmashed;
            }
        }

        public void Tick()
        {
            if (owner == null) return;

            if (fish != null)
            { 
                if (owner.CanEat(fish) == true)
                {
                    if (collisionHandler != null)
                    {
                        collisionHandler.OnTerrainSmashed -= HandleTerrainSmashed;
                    }
                    
                    owner.ChangeState(new FishBaitState());
                }
            }
    
            if (player != null)
            { 
                owner.FishMovement.StopMove();
                
                idleTimer += Time.deltaTime;
                
                if(idleTimer >= 3f)
                    owner.FishMovement.Move(rushDirection, rushSpeed);
                
                if(idleTimer >= 10f)
                    Exit();
            }
        }

        private void HandleTerrainSmashed(float impactStrength)
        {
            if (owner == null) return;
            
            float damageToResistance = impactStrength * collisionRatio; 
            owner.DecreaseResistance(damageToResistance);
            owner.FishMovement.StopMove();
            Exit();
            Debug.Log($"지형 충돌! 저항도 {damageToResistance:F1} 감소.");
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
                owner.ChangeState(new FishIdleState());
            }
        }
    }
}