using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace WeGotOne
{
    public sealed class FishObjectPool : SceneSingleton<FishObjectPool>
    {
        [SerializeField] private WorldItem[] fishPrefabs;
        [SerializeField] private WorldItem[] plankPrefabs = Array.Empty<WorldItem>();

        private readonly Dictionary<int, Queue<WorldItem>> inactiveFishByItemId = new();
        private readonly HashSet<WorldItem> activeFish = new();
        private readonly Dictionary<int, Queue<WorldItem>> inactivePlanksByItemId = new();
        private readonly HashSet<WorldItem> activePlanks = new();
        private WorldItemRegistry worldItemRegistry;

        protected override void OnInitialize()
        {
            base.OnInitialize();
            worldItemRegistry = GetComponent<WorldItemRegistry>();
        }

        private async void Start()
        {
            await UniTask.WaitUntil(() => WorldItemRegistry.IsInitialized);
            worldItemRegistry = WorldItemRegistry.Instance;
        }

        public WorldItem Get(int itemId)
        {
            if (inactiveFishByItemId.TryGetValue(itemId, out Queue<WorldItem> inactiveFish))
            {
                while (inactiveFish.Count > 0)
                {
                    WorldItem pooledFish = inactiveFish.Dequeue();
                    if (pooledFish == null)
                        continue;

                    InitializeFish(pooledFish);
                    activeFish.Add(pooledFish);
                    return pooledFish;
                }
            }

            WorldItem prefab = FindFishPrefab(itemId);
            if (prefab == null)
            {
                Debug.LogError($"[ItemObjectPool] ItemId {itemId}에 해당하는 Fish 프리팹을 찾을 수 없습니다.");
                return null;
            }

            WorldItem fish = Instantiate(prefab);
            InitializeFish(fish);
            activeFish.Add(fish);
            return fish;
        }

        public void Release(WorldItem fish)
        {
            if (fish == null)
                return;

            if (fish.TryGetComponent<Fish>(out _) == false)
            {
                Debug.LogWarning($"[ItemObjectPool] Fish가 아닌 WorldItem은 반환할 수 없습니다: {fish.name}");
                return;
            }

            fish.SetIsPickUp(false);
            fish.DeactivateFromSnapshot();

            if (fish.WorldItemId >= 0)
            {
                worldItemRegistry.UnregisterDynamic(fish.WorldItemId, out _);
            }

            activeFish.Remove(fish);

            if (inactiveFishByItemId.TryGetValue(fish.ItemId, out Queue<WorldItem> inactiveFish) == false)
            {
                inactiveFish = new Queue<WorldItem>();
                inactiveFishByItemId.Add(fish.ItemId, inactiveFish);
            }

            inactiveFish.Enqueue(fish);
        }

        public void ReleaseAllActiveFish()
        {
            var fishes = new List<WorldItem>(activeFish);
            foreach (WorldItem fish in fishes)
            {
                Release(fish);
            }
        }

        public WorldItem GetPlank(int itemId)
        {
            if (inactivePlanksByItemId.TryGetValue(itemId, out Queue<WorldItem> inactivePlanks))
            {
                while (inactivePlanks.Count > 0)
                {
                    WorldItem pooledPlank = inactivePlanks.Dequeue();
                    if (pooledPlank == null)
                        continue;

                    InitializePlank(pooledPlank);
                    activePlanks.Add(pooledPlank);
                    return pooledPlank;
                }
            }

            WorldItem plank = Instantiate(FindPlankPrefab(itemId));
            InitializePlank(plank);
            activePlanks.Add(plank);
            return plank;
        }

        public void ReleasePlank(WorldItem plank)
        {
            if (plank.TryGetComponent<Plank>(out _) == false ||
                worldItemRegistry.IsFixedWorldItem(plank.WorldItemId))
            {
                throw new InvalidOperationException($"[FishObjectPool] 동적 Plank만 반환할 수 있습니다: {plank.name}");
            }

            plank.SetIsPickUp(false);
            plank.DeactivateFromSnapshot();
            if (plank.WorldItemId >= 0)
            {
                worldItemRegistry.UnregisterDynamic(plank.WorldItemId, out _);
            }

            activePlanks.Remove(plank);
            if (inactivePlanksByItemId.TryGetValue(plank.ItemId, out Queue<WorldItem> inactivePlanks) == false)
            {
                inactivePlanks = new Queue<WorldItem>();
                inactivePlanksByItemId.Add(plank.ItemId, inactivePlanks);
            }

            inactivePlanks.Enqueue(plank);
        }

        public void ReleaseAllActivePlanks()
        {
            var planks = new List<WorldItem>(activePlanks);
            foreach (WorldItem plank in planks)
            {
                ReleasePlank(plank);
            }
        }

        private WorldItem FindPlankPrefab(int itemId)
        {
            foreach (WorldItem prefab in plankPrefabs)
            {
                if (prefab.ItemId == itemId && prefab.TryGetComponent<Plank>(out _))
                    return prefab;
            }

            throw new InvalidOperationException($"[FishObjectPool] ItemId {itemId}에 해당하는 Plank 프리팹을 찾을 수 없습니다.");
        }

        private void InitializePlank(WorldItem plank)
        {
            plank.ResetTransformSnapshot();
            plank.SetIsPickUp(false);
            plank.gameObject.SetActive(true);
        }

        private WorldItem FindFishPrefab(int itemId)
        {
            foreach (WorldItem prefab in fishPrefabs)
            {
                if (IsFishPrefab(prefab) && prefab.ItemId == itemId)
                    return prefab;
            }
        
            return null;
        }
        
        private bool IsFishPrefab(WorldItem prefab)
        {
            return prefab != null && prefab.TryGetComponent<Fish>(out _);
        }
        
        private void InitializeFish(WorldItem fish)
        {
            fish.ResetTransformSnapshot();
            fish.SetIsPickUp(false);
            fish.gameObject.SetActive(true);
        }
    }
}
