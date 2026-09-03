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

        public void SeahorseGrab(GameObject other)
        {
            manager.PlayerRootPosition = other.transform.root.GetChild(0);
            manager.ChangeState(new FishItemState());
        }

        public void SeahorseRelease()
        {
            manager.PlayerRootPosition.GetComponent<Rigidbody2D>().gravityScale = 1;
            manager.PlayerRootPosition = null;
            manager.ObjectPool.Release(gameObject.GetComponent<WorldItem>());
        }
    }
}