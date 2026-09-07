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

#if UNITY_EDITOR
        private void OnValidate()
        {
            // OnValidate can run during loading; use the editor main thread for Unity APIs.
            UnityEditor.EditorApplication.delayCall -= UpdatePreviewAsync;
            UnityEditor.EditorApplication.delayCall += UpdatePreviewAsync;
        }

        // 에디터 전용이라 예외처리 안함
        private async void UpdatePreviewAsync()
        {
            if (this == null || Application.isPlaying || UnityEditor.EditorApplication.isPlayingOrWillChangePlaymode)
                return;

            await ItemEditorCache.EnsureLoadedAsync();

            // Loading may finish after the object was destroyed or Play Mode began.
            if (this == null || Application.isPlaying || UnityEditor.EditorApplication.isPlayingOrWillChangePlaymode)
                return;

            SpriteRenderer previewRenderer = GetComponent<SpriteRenderer>();
            previewRenderer.enabled = false;
            ItemDto itemDto = ItemEditorCache.Get(itemId);
            if (string.IsNullOrWhiteSpace(itemDto.IconPath))
            {
                throw new System.InvalidOperationException($"ItemId {itemId}의 IconPath가 비어 있습니다.");
            }

            Sprite icon = Resources.Load<Sprite>(itemDto.IconPath);
            if (icon == null)
            {
                throw new System.InvalidOperationException(
                    $"ItemId {itemId}의 Icon을 찾을 수 없습니다: {itemDto.IconPath}");
            }

            previewRenderer.sprite = icon;
            Color color = previewRenderer.color;
            color.a = 0.35f;
            previewRenderer.color = color;
            previewRenderer.enabled = true;
            UnityEditor.SceneView.RepaintAll();
        }
#endif
        
        private void Awake()
        {
            if (Application.isPlaying)
                GetComponent<SpriteRenderer>().enabled = false; 
            
            waterArea = GameObject.Find("WaterBorderLine").GetComponent<Collider2D>();
        }

        private IEnumerator Start()
        {
            yield return new WaitUntil(() => ItemManager.Instance.IsInitialized && WorldItemRegistry.IsInitialized);
            fishItem = ItemManager.Instance.Get(itemId);
            worldItemRegistry = WorldItemRegistry.Instance;
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
