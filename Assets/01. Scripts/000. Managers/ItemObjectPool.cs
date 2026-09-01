using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace WeGotOne
{
    public class ItemObjectPool : MonoBehaviour
    {
        [SerializeField] private WorldItem[] fishPrefabs;
        [SerializeField] private Transform[] itemTransforms;
        // 물고기 소환 시 제한 영역 설정용
        [SerializeField] private Collider2D waterArea;

        // 맵에 미리 배치된 나무 정보 가져오기
        private List<Transform> plankList;


        #region public method

        public IReadOnlyList<Transform> InitialFishSpawnPoints => itemTransforms;

        public bool TryGetRandomFishItemId(out int itemId)
        {
            List<WorldItem> validPrefabs = new();
            foreach (WorldItem prefab in fishPrefabs)
            {
                if (IsFishPrefab(prefab))
                {
                    validPrefabs.Add(prefab);
                }
            }

            if (validPrefabs.Count == 0)
            {
                itemId = -1;
                Debug.LogError("[ItemObjectPool] Fish 프리팹이 설정되지 않았습니다.");
                return false;
            }

            itemId = validPrefabs[Random.Range(0, validPrefabs.Count)].ItemId;
            return true;
        }

        private void RegisterExistingPlanks()
        {
            Plank[] planks = FindObjectsOfType<Plank>();

            PlankCount = planks.Length;

            for (int i = 0; i < PlankCount; i++)
            {
                Plank plank = planks[i];
                if (RoomClient.Instance.Me.IsHost == false)
                {
                    plank.GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Kinematic;
                }

                PoolObjectDic[itemId++] = plank;

            } 
        }

        public void ItemSpawn()
        {
            for (int i = 0; i < itemTransforms.Length; i++)
            {
                // 판자는 맵에 이미 존재하므로 생성하지 않음
                // if (i < PlankCount)
                //     continue;

                Transform currentTransform = itemTransforms[i];
                bool isExist = currentTransform.childCount > 0;

                if (!isExist)
                {
                    int prefabIndex = Random.Range(1, itemPrefabs.Length);

                    Collectible collectible =
                        Instantiate(itemPrefabs[prefabIndex], currentTransform);

                    collectible.transform.position = currentTransform.position;
                    
                    FishMovement water = collectible.GetComponent<FishMovement>();
                    
                    Debug.Log(water.gameObject.name);

                    if (water != null)
                    {
                        water.SetWaterArea(waterArea);
                    }
                    
                    // [게스트 전용 - 스폰 즉시 물리/이동 차단] - 신규 (요구사항 1, 2)
                    // Plank는 이동 로직이 없으므로 물고기(ItemType != 0)만 대상으로 함
                    
                    if (RoomClient.Instance.Me.IsHost == false && collectible.ThisItem.ItemType == ItemType.Fish)
                    {
                        var movement = collectible.GetComponent<FishMovement>();
                        if (movement != null) movement.LockForGuest();
                    }
                    
                    
                    
                    // [개선]: 인덱서([])를 사용하여 중복 키 오류 없이 안전하게 저장
                    PoolObjectDic[itemId++] = collectible; 
            
                    continue;
                }

                GameObject item = currentTransform.GetChild(0).gameObject;

                if (!item.activeSelf)
                {
                    item.SetActive(true);
                    item.transform.position = currentTransform.position;

                    Debug.Log($"[Pool] 아이템 재활용 활성화 - 슬롯 인덱스: {i}");
                }
            }
        }
        
        #region 네트워크 동기화 전용 기능 (Host 측 패킹)

        /// <summary>
        /// [호스트 전용] 현재 씬에 활성화되어 작동 중인 아이템(물고기)들의 트랜스폼 상태를 
        /// 네트워크 전송 규격(FishState) 목록으로 안전하게 조립합니다.
        /// </summary>
        public List<FishState> PackActiveFishes()
        {
            var list = new List<FishState>();

            foreach (var pair in PoolObjectDic)
            {
                Collectible collectible = pair.Value;
                if (collectible == null) continue;
                if (collectible.ThisItem.ItemType != ItemType.Fish) continue; // 물고기만 포함
                
                list.Add(new FishState
                {
                    Id = $"Slot_{pair.Key}",
                    FishTypeIndex = collectible.ThisItem.Key, // ItemType(대분류)이 아닌 Key(개별 식별값) 사용
                    IsActive = collectible.gameObject.activeSelf,
                    X = collectible.transform.position.x,
                    Y = collectible.transform.position.y,
                    ScaleX = collectible.transform.localScale.x,
                    Angle = collectible.transform.eulerAngles.z,
                    CurrentState = "Active" 
                });
            }

            return list;
        }

        public List<PlankState> PackActivePlanks()
        {
            var list = new List<PlankState>();

            foreach (var pair in PoolObjectDic)
            {
                Collectible collectible = pair.Value;
                if (collectible == null) continue;
                if (collectible.ThisItem.ItemType != ItemType.Plank) continue; // Plank만 포함
                

                list.Add(new PlankState
                {
                    Id = $"Slot_{pair.Key}",
                    IsActive = collectible.gameObject.activeSelf,
                    Index = collectible.ThisItem.Key,
                    X = collectible.transform.position.x,
                    Y = collectible.transform.position.y,
                    Angle = collectible.transform.eulerAngles.z,
                    IsPickUp = collectible.IsPickUp
                });
            }

            return list;
        }

        public void ReleaseFish(WorldItem fish)
        {
            if (fish == null)
                return;

            if (fish.TryGetComponent<Fish>(out _) == false)
            {
                Debug.LogError($"[ItemObjectPool] Fish가 아닌 WorldItem은 반환할 수 없습니다: {fish.name}");
                return;
            }

            fish.SetIsPickUp(false);
            fish.DeactivateFromSnapshot();

            if (inactiveFishByItemId.TryGetValue(fish.ItemId, out Queue<WorldItem> inactiveFish) == false)
            {
                inactiveFish = new Queue<WorldItem>();
                inactiveFishByItemId.Add(fish.ItemId, inactiveFish);
            }

            inactiveFish.Enqueue(fish);
        }

        #endregion

        #region private method

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
            return prefab != null && prefab.ThisItem != null &&
                   prefab.ThisItem.ItemType == ItemType.Fish &&
                   prefab.TryGetComponent<Fish>(out _);
        }
        
        private void InitializeFish(WorldItem fish)
        {
            fish.ResetTransformSnapshot();
            fish.SetIsPickUp(false);
            fish.gameObject.SetActive(true);
        }

        #endregion
    }
}
