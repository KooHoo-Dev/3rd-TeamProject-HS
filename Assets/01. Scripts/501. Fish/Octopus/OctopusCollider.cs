using UnityEngine;
using WeGotOne.Fish_Interface;

namespace WeGotOne
{
    public class OctopusCollider : MonoBehaviour
    {
        private FishStateManager fishStateManager;
        private PlayerController target;

        private void OnEnable()
        {
            fishStateManager = GetComponentInParent<FishStateManager>();
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.gameObject.layer == LayerMask.NameToLayer("Player"))
            {
                target = other.gameObject.GetComponentInParent<PlayerController>();
                if(fishStateManager.CurrentState is FishOctopusState == false)
                    fishStateManager.ChangeState(new FishOctopusState(target.Body.transform));
            }
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (target == null)
                return;

            if (other.gameObject != target.gameObject)
                return;

            target = null;
            fishStateManager.ChangeState(new FishIdleState());
        }
    }
}