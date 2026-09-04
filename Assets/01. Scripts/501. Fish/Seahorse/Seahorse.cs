using System;
using UnityEngine;
using WeGotOne.Fish_Interface;

namespace WeGotOne
{
    public class Seahorse : MonoBehaviour
    {
        private FishStateManager manager;

        private void Awake()
        {
            manager = gameObject.GetComponent<FishStateManager>();
        }

        public void SeahorseGrab()
        {
            manager.ChangeState(new AbductionState());
        }

        public void SeahorseRelease()
        {
            
            manager.ObjectPool.Release(gameObject.GetComponent<WorldItem>());
        }
    }
}