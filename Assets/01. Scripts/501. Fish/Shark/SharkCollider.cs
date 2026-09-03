using UnityEngine;
using WeGotOne.Fish_Interface;

namespace WeGotOne
{
    public class SharkCollider : MonoBehaviour
    {
        private FishStateManager fishStateManager;
        private Collider2D target;

        private void OnEnable()
        {
            fishStateManager = GetComponentInParent<FishStateManager>();
        }
    
        private void OnTriggerEnter2D(Collider2D other)
        {
            target = other;
            if(other.GetComponentInParent<PlayerController>() != null || other.TryGetComponent(out FishStateManager fish) == true)
                fishStateManager.ChangeState(new Shark(other));
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (target == null)
                return;

            if (other.gameObject != target.gameObject)
                return;

            target = null;
        }
    }
}