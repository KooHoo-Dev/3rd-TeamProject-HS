using System;
using UnityEngine;
using WeGotOne.Fish_Interface;

namespace WeGotOne
{
    public class FishBaitCollider : MonoBehaviour
    {
        [SerializeField] private FishStateManager fishStateManager;
        [SerializeField] private Collider2D detectingRange;

        private void Awake()
        {
            fishStateManager = GetComponentInParent<FishStateManager>();
            detectingRange = GetComponent<Collider2D>();
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.TryGetComponent(out FishStateManager fishState))
            {
                if (fishState.baitType == fishStateManager.baitType)
                {
                    fishStateManager.BaitPosition = fishState.transform.position;
                    fishStateManager.ChangeState(new FishBaitState());
                }
            }
        }
    }
}