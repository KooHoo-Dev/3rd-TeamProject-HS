using UnityEngine;

namespace WeGotOne
{
    public class WaterZone : MonoBehaviour
    {
        // 물속
        [SerializeField] private float waterDamping = 0.8f;
        [SerializeField] private float waterGravity = 0.8f;
        
        // 물밖
        [SerializeField] private float airDamping = 0.5f;
        [SerializeField] private float airGravity = 1f;


        private void OnTriggerEnter2D(Collider2D other)
        {
            Rigidbody2D rb = other.attachedRigidbody;

            if (rb == null)
                return;

            rb.linearDamping = waterDamping;
            rb.gravityScale = waterGravity;
        }


        private void OnTriggerExit2D(Collider2D other)
        {
            Rigidbody2D rb = other.attachedRigidbody;

            if (rb == null)
                return;

            rb.linearDamping = airDamping;
            rb.gravityScale = airGravity;
        }
    }
}