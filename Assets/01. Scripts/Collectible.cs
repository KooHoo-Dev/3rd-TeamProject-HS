using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace WeGotOne
{
    public abstract class Collectible : MonoBehaviour
    {
        [field: SerializeField] public ItemScriptableObject ThisItem { get; private set; }
        protected PoolingSystem<Collectible> myPool;

        private void Update()
        {
            if (SimpleInput.GetKeyDown(Key.Digit2))
                OnPickup();
        }

        public virtual void Init(PoolingSystem<Collectible> pool, Vector3 randomTransform)
        {
            myPool = pool;
            transform.position = randomTransform;
        }

        public virtual void OnPickup()
        {
            myPool.Release(this);
        }
    }
}