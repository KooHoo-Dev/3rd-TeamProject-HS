using System;
using UnityEngine;
using UnityEngine.InputSystem.iOS;
using WeGotOne.Fish_Interface;

namespace WeGotOne
{
    public class SeaHorse : MonoBehaviour
    {
        [SerializeField] private FishStateManager manager;
        [SerializeField] private HandControllerV2 playerHand;

        [SerializeField] private float moveSpeed;

        private void Awake()
        {
            manager = GetComponent<FishStateManager>();
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.gameObject.layer == LayerMask.NameToLayer("PlayerHand"))
            {
                playerHand = other.GetComponent<HandControllerV2>();

                if (playerHand.IsGrabbing)
                {
                    manager.ChangeState(new FishItemState());
                }
            }
        }

        private void OnTriggerStay2D(Collider2D other)
        {
            if (other.gameObject.layer == LayerMask.NameToLayer("PlayerHand"))
            {
                if (playerHand != null && playerHand.IsGrabbing)
                {
                    Vector2 destination = manager.AbductionDestination.position - manager.transform.position;
                    float distance =
                        Vector2.Distance(manager.transform.position, manager.AbductionDestination.position);
                    Debug.Log(manager.AbductionDestination.name);

                    manager.FishMovement.Move(destination, moveSpeed);
                    Debug.Log(destination);

                    if (distance < 1.0f)
                    {
                        playerHand.Release();
                        playerHand = null;
                    }
                }
            }
        }
    }
}