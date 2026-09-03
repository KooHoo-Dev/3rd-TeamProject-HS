using UnityEngine;

namespace WeGotOne
{
    public class Bullet : MonoBehaviour
    {
        [Header("Hit Effect")]
        [SerializeField] private ParticleSystem hitEffect;

        private Vector2 direction;
        private float speed;

        private bool isFlying;
        private bool hasHit;

        public void Fire(Vector2 direction, float speed)
        {
            this.direction = direction;
            this.speed = speed;

            isFlying = true;
            hasHit = false;

            if (hitEffect != null)
            {
                hitEffect.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            }
        }

        private void Update()
        {
            if (isFlying)
            {
                transform.position += (Vector3)(direction * speed * Time.deltaTime);
            }
            if (hasHit && hitEffect != null && !hitEffect.IsAlive(true))
            {
                gameObject.SetActive(false);
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (hasHit)
                return;
            
            int hitLayer = other.gameObject.layer;
            int playerLayer = LayerMask.NameToLayer("Player");
            int wallLayer = LayerMask.NameToLayer("Wall");
            
            if (hitLayer != playerLayer && hitLayer != wallLayer)
            {
                return;
            }
            
            Hit();
        }

        private void Hit()
        {
            hasHit = true;

            isFlying = false;

            SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
            
            if (spriteRenderer != null)
            {
                spriteRenderer.enabled = false;
            }

            Collider2D bulletCollider = GetComponent<Collider2D>();
            
            if (bulletCollider != null)
            {
                bulletCollider.enabled = false;
            }

            if (hitEffect != null)
            {
                hitEffect.Play();
            }
            else
            {
                gameObject.SetActive(false);
            }
        }
    }
}