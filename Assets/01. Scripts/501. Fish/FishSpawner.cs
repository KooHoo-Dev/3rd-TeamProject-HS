using System.Collections;
using HelloServer;
using UnityEngine;

namespace WeGotOne
{
    public sealed class FishSpawner : MonoBehaviour
    {
        [SerializeField] private ItemScriptableObject fishItem;
        [SerializeField] private Collider2D waterArea;

        private WorldItem fish;
        
        private WorldItemRegistry worldItemRegistry;

        private SpriteRenderer spriteRenderer;

        private void OnValidate()
        {
            SpriteRenderer renderer = GetComponent<SpriteRenderer>();

            if (renderer == null)
                return;

            renderer.sprite = fishItem?.Icon;
            renderer.color = new Color(1f, 1f, 1f, 0.35f);
            renderer.enabled = true;
        } 
        
        private void Awake()
        {
            if (Application.isPlaying)
                GetComponent<SpriteRenderer>().enabled = false; 
            
            worldItemRegistry = FindFirstObjectByType<WorldItemRegistry>();
            
            waterArea = GameObject.Find("WaterBorderLine").GetComponent<Collider2D>();
        }

        /*private IEnumerator Start()
        {
            yield return new WaitUntil(() => RoomClient.Instance != null && RoomClient.Instance.Me != null);

            if (RoomClient.Instance.Me.IsHost)
            {
                Spawn();
            }
        }*/
        
        private void OnEnable()
        {
            StartCoroutine(WaitAndSpawn());
        }

        private IEnumerator WaitAndSpawn()
        {
            yield return new WaitUntil(() => RoomClient.Instance != null && RoomClient.Instance.Me != null);

            if (RoomClient.Instance.Me.IsHost)
            {
                Spawn();
            }
        }

        private void Spawn()
        {
            if (fish == null)
                fish = FishObjectPool.Instance.Get(fishItem.Key);

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
