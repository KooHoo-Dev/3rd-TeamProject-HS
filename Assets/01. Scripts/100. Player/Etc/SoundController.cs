using UnityEngine;

namespace WeGotOne
{
    public class SoundController : MonoBehaviour
    {
        private Rigidbody2D rb;
        private int waterLayer;

        private void Start()
        {
            rb = GetComponent<Rigidbody2D>();
            waterLayer = LayerMask.NameToLayer("Water");
        }
        
        private void OnTriggerEnter2D(Collider2D other)
        {
            if (rb.linearVelocity.magnitude >= 10f)
            {
                if(other.gameObject.layer == waterLayer)
                    AudioManager.Instance.PlaySfx(AudioManager.Sfx.SplashWater);
                else
                    AudioManager.Instance.PlaySfx(AudioManager.Sfx.HitRock);
            }
        }
    }
}