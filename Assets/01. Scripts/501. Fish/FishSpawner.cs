using System.Collections;
using UnityEngine;

namespace WeGotOne
{
    public sealed class FishSpawner : MonoBehaviour
    {
        [SerializeField] private int itemId;
        [SerializeField] private Collider2D waterArea;

        private ItemRef fishItem;
        private WorldItemRegistry worldItemRegistry;
        
        private void Awake()
        {
            if (Application.isPlaying)
                GetComponent<SpriteRenderer>().enabled = false; 
            
            waterArea = GameObject.Find("WaterBorderLine").GetComponent<Collider2D>();
        }

        private IEnumerator Start()
        {
            yield return new WaitUntil(() => ItemManager.Instance.IsLoaded && WorldItemRegistry.IsInitialized);
            worldItemRegistry = WorldItemRegistry.Instance;
            fishItem = ItemManager.Instance.Get(itemId);
        }

        public void Spawn()
        {
            WorldItem fish = FishObjectPool.Instance.Get(fishItem.ItemId);

            fish.transform.position = transform.position;
            if (fish.TryGetComponent(out FishMovement fishMovement))
            {
                fishMovement.SetWaterArea(waterArea);
            }

            if (worldItemRegistry.RegisterNewDynamic(fish, out _) == false)
            {
                FishObjectPool.Instance.Release(fish);
            }
        }
    }
}
