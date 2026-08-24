using UnityEngine;

namespace WeGotOne
{
    public class ItemPoolingManager : SingletonBase<ItemPoolingManager>
    {
        [SerializeField] private GameObject[] itemPrefabs;
        
        [field: SerializeField] public int MaxPoolSize { get; private  set; }

        protected override void OnInitialize()
        {
            base.OnInitialize();

            for (int i = 0; i < itemPrefabs.Length; i++)
            {
                PoolingSystem poolingSystem = new PoolingSystem(itemPrefabs[i]);
            }
        }
        
        
    }   
}