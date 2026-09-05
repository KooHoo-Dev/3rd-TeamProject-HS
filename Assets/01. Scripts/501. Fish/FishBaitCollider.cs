using System;
using UnityEngine;
using WeGotOne.Fish_Interface;

namespace WeGotOne
{
    public class FishBaitCollider : MonoBehaviour
    {
        [SerializeField] private FishStateManager fishStateManager;
        private FishStateManager target;
        [SerializeField] private Collider2D detectingRange;

        private void Awake()
        {
            fishStateManager = GetComponentInParent<FishStateManager>();
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.TryGetComponent(out FishStateManager fishState))
            {
                if (fishStateManager.CanEat(fishState))
                {
                    fishStateManager.ChangeState(new FishBaitState());
                }
            }
        }
        
        private void OnTriggerExit2D(Collider2D other)
        {
            if (other.TryGetComponent(out FishStateManager fishState))
            {
                if (fishState == fishStateManager.BaitInfo)
                {
                    fishStateManager.BaitInfo = null;
                }
            }
        }
    }
}