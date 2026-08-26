using UnityEngine;
using UnityEngine.InputSystem;

namespace WeGotOne
{
    public abstract class Collectible : MonoBehaviour
    {
        [field: SerializeField]
        public ItemScriptableObject ThisItem { get; private set; }
        protected GameObjectPool<Collectible> myPool;
        
        public virtual void Init(GameObjectPool<Collectible> pool, Vector3 randomTransform)
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