using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace WeGotOne
{
    public abstract class Collectible : MonoBehaviour
    {
        [field: SerializeField] public ItemScriptableObject ThisItem { get; private set; }
        
        protected PoolingSystem<Collectible> myPool; 
        public bool IsPickUp { get; set; } = false;

        private void Update()
        {
            if (SimpleInput.GetKeyDown(Key.Digit2))
                OnPickup();
        }

        public virtual void Init(PoolingSystem<Collectible> pool, Transform parent)
        {
            myPool = pool;
            transform.SetParent(parent);
            transform.position = parent.position;
        }

        public virtual void OnPickup()
        {
            gameObject.SetActive(false);
        }
    }
}