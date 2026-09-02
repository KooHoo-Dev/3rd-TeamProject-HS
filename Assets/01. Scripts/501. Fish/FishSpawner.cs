using System.Collections;
using HelloServer;
using UnityEngine;

namespace WeGotOne
{
    public sealed class FishSpawner : MonoBehaviour
    {
        [SerializeField] private ItemScriptableObject fishItem;
        [SerializeField] private Collider2D waterArea;

        private FishObjectPool fishObjectPool;
        private WorldItemRegistry worldItemRegistry;

        private void Awake()
        {
            fishObjectPool = FindFirstObjectByType<FishObjectPool>();
            
            worldItemRegistry = FindFirstObjectByType<WorldItemRegistry>();
            
            waterArea = GameObject.Find("WaterBorderLine").GetComponent<Collider2D>();
        }

        private IEnumerator Start()
        {
            yield return new WaitUntil(() => RoomClient.Instance != null && RoomClient.Instance.Me != null);

            if (RoomClient.Instance.Me.IsHost)
            {
                Spawn();
            }
        }

        private void Spawn()
        {
            WorldItem fish = fishObjectPool.Get(fishItem.Key);
            if (fish == null)
                return;

            fish.transform.position = transform.position;
            if (fish.TryGetComponent(out FishMovement fishMovement))
            {
                fishMovement.SetWaterArea(waterArea);
            }

            if (worldItemRegistry.RegisterNewDynamic(fish, out _) == false)
            {
                fishObjectPool.Release(fish);
            }
        }
    }
}
