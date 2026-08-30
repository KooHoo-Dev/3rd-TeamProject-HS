using System.Collections;
using System.Collections.Generic;
using HelloServer;
using UnityEngine;
using WeGotOne.Fish_Interface;
using Random = UnityEngine.Random;

namespace WeGotOne
{
    public class ItemObjectPool : MonoBehaviour
    {
        [field: SerializeField] public int PlankCount { get; set; }
        
        [SerializeField] private Collectible[] itemPrefabs;
        
        // [개선]: 인덱스(스폰 위치 순서)를 키로 삼아 생성된 아이템을 매핑·관리하는 딕셔너리
        public Dictionary<int, Collectible> PoolObjectDic { get; private set; } 
            = new();

        [SerializeField] private Transform[] itemTransforms;

        private bool isHost; // isHost 판단용
        
        private IEnumerator Start()
        {   
            // 다리 개수에 맞춰 판자 생성 제한 설정
            yield return new WaitUntil(() => RoomClient.Instance.SnapshotMessage != null);
            isHost = RoomClient.Instance.Me != null && RoomClient.Instance.Me.IsHost; // bool 값 할당
            ItemSpawn();
        }

        public void ItemSpawn()
        {
            for (int i = 0; i < itemTransforms.Length; i++)
            {
                Transform currentTransform = itemTransforms[i];
                bool isExist = currentTransform.childCount > 0;

                // 1. 해당 자리에 아이템이 없는 경우 (새로 생성 후 딕셔너리 등록)
                if (!isExist)
                {
                    // i가 PlankCount보다 작으면 0번 프리팹(판자), 아니면 랜덤 프리팹(물고기 등)
                    int prefabIndex = (i < PlankCount) ? 0 : Random.Range(1, itemPrefabs.Length);
            
                    Collectible collectible = Instantiate(itemPrefabs[prefabIndex], currentTransform);
                    collectible.transform.position = currentTransform.position;
                    
                    
                    // [게스트 전용 - 스폰 즉시 물리/이동 차단] - 신규 (요구사항 1, 2)
                    // Plank는 이동 로직이 없으므로 물고기(ItemType != 0)만 대상으로 함
                    
                    if (isHost == false && collectible.ThisItem.ItemType == ItemType.Fish)
                    {
                        var movement = collectible.GetComponent<FishMovement>();
                        if (movement != null) movement.LockForGuest();
                    }
                    
                    
                    
                    // [개선]: 인덱서([])를 사용하여 중복 키 오류 없이 안전하게 저장
                    PoolObjectDic[i] = collectible; 
            
                    Debug.Log($"[Pool] 새 아이템 생성 - 슬롯 인덱스: {i}");
                    continue; // 생성을 완료했으므로 재활용 로직은 건너뜀
                }

                // 2. 이미 아이템이 존재하지만 비활성화 상태인 경우 (재활용하여 다시 켜기)
                GameObject item = currentTransform.GetChild(0).gameObject;
        
                if (!item.activeSelf)
                {
                    Plank plank = item.GetComponent<Plank>();
            
                    // 만약 주울 수 있는 판자 상태인데 이미 먹힌 경우라면 대기
                    if (plank != null && plank.IsPickUp)
                    {
                        continue;
                    }
            
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
                if (collectible == null || !collectible.gameObject.activeSelf) continue;
                if (collectible.ThisItem.ItemType != ItemType.Fish) continue; // 물고기만 포함

                list.Add(new FishState
                {
                    Id = $"Slot_{pair.Key}",
                    FishTypeIndex = collectible.ThisItem.Key, // ItemType(대분류)이 아닌 Key(개별 식별값) 사용
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
                if (collectible == null || !collectible.gameObject.activeSelf) continue;
                if (collectible.ThisItem.ItemType != ItemType.Plank) continue; // Plank만 포함
                

                list.Add(new PlankState
                {
                    Id = $"Slot_{pair.Key}",
                    X = collectible.transform.position.x,
                    Y = collectible.transform.position.y,
                    Angle = collectible.transform.eulerAngles.z,
                    IsPickUp = collectible.IsPickUp
                });
            }

            return list;
        }

        /// <summary>
        /// 스폰된 클론 오브젝트가 원본 itemPrefabs 배열 중 몇 번 인덱스인지 역추적합니다.
        /// </summary>
        /*private int GetPrefabTypeIndex(Collectible target)
        {
            if (target == null) return 0;
            
            for (int i = 0; i < itemPrefabs.Length; i++)
            {
                // Instantiate 시 생성되는 '(Clone)' 접미사를 무시하기 위해 Contains 비교를 활용합니다.
                if (target.name.Contains(itemPrefabs[i].name))
                {
                    return i;
                }
            }
            return 0;
        }*/

        #endregion
        
        # region 신성건
        /// <summary>
        /// [게스트 전용] 수신한 호스트 측 물고기 스냅샷을 내 화면의 물고기 풀에 반영합니다.
        /// 스폰 시 게스트가 호스트와 다른 종류를 랜덤으로 뽑았을 경우, 프리팹 자체를
        /// 올바른 종류로 교체한 뒤 좌표/스케일/회전을 적용합니다.
        /// </summary>
        public void ApplyFishes(FishState[] fishes)
        {
            for (int i = 0; i < fishes.Length; i++)
            {
                FishState state = fishes[i];

                if (TryGetSlotIndex(state.Id, out int slotIndex) == false)
                    continue;

                if (PoolObjectDic.TryGetValue(slotIndex, out Collectible collectible) == false)
                    continue;

                // ==========================================
                // [프리팹 종류 불일치 보정] - 신규
                // 게스트가 스폰 시 랜덤으로 뽑은 물고기 종류(Key)가 호스트와 다를 수 있으므로,
                // 스냅샷의 FishTypeIndex와 실제 오브젝트의 Key가 다르면 올바른 프리팹으로 교체합니다.
                // ==========================================
                if (collectible.ThisItem.Key != state.FishTypeIndex)
                {
                    collectible = ReplaceWithCorrectPrefab(slotIndex, collectible, state.FishTypeIndex);
                    if (collectible == null) continue; // 방어: 해당 Key의 프리팹을 못 찾은 경우
                }

                collectible.transform.position = new Vector3(state.X, state.Y, collectible.transform.position.z);
                collectible.transform.localScale = new Vector3(state.ScaleX, collectible.transform.localScale.y, collectible.transform.localScale.z);
                collectible.transform.rotation = Quaternion.Euler(0f, 0f, state.Angle);
            }
        }
        
        
        public void ApplyPlanks(PlankState[] planks)
        {
            for (int i = 0; i < planks.Length; i++)
            {
                PlankState state = planks[i];

                if (TryGetSlotIndex(state.Id, out int slotIndex) == false)
                    continue;

                if (PoolObjectDic.TryGetValue(slotIndex, out Collectible collectible) == false)
                    continue;

                collectible.transform.position = new Vector3(state.X, state.Y, collectible.transform.position.z);
                collectible.transform.rotation = Quaternion.Euler(0f, 0f, state.Angle);
                collectible.IsPickUp = state.IsPickUp;
            }
        }
        
        /// <summary>
        /// [게스트 전용] 슬롯에 이미 스폰된 오브젝트를 파괴하고, FishTypeIndex(Key)에 해당하는
        /// 올바른 프리팹으로 다시 생성하여 딕셔너리를 갱신합니다.
        /// </summary>
        private Collectible ReplaceWithCorrectPrefab(int slotIndex, Collectible oldCollectible, int correctKey)
        {
            Collectible correctPrefab = null;
            foreach (var prefab in itemPrefabs)
            {
                if (prefab.ThisItem.Key == correctKey)
                {
                    correctPrefab = prefab;
                    break;
                }
            }

            if (correctPrefab == null)
            {
                Debug.LogWarning($"[Pool] Key {correctKey}에 해당하는 프리팹을 찾을 수 없습니다.");
                return null;
            }

            Transform parent = oldCollectible.transform.parent;
            Vector3 pos = oldCollectible.transform.position;

            Destroy(oldCollectible.gameObject);

            Collectible newCollectible = Instantiate(correctPrefab, parent);
            newCollectible.transform.position = pos;

            // 게스트이므로 새로 생성된 물고기도 즉시 이동/물리 잠금 적용
            var movement = newCollectible.GetComponent<FishMovement>();
            if (movement != null) movement.LockForGuest();

            PoolObjectDic[slotIndex] = newCollectible;
            return newCollectible;
        }
        
        
        // 아이템 값 저장할때 Slot_{슬롯번호}식으로 들어가길래 파싱해주는 용.
        // 그냥 형식 맞나만 보고 앞에서5개 빼버렷어요
        private bool TryGetSlotIndex(string id, out int slotIndex)
        {
            slotIndex = -1;

            if (string.IsNullOrEmpty(id))
                return false;

            if (id.StartsWith("Slot_") == false)
                return false;

            return int.TryParse(id.Substring(5), out slotIndex);
        }
        
        # endregion
        
    }
}