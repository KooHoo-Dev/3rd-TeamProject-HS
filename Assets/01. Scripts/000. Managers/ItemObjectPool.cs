using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace WeGotOne
{
    public class ItemObjectPool : MonoBehaviour
    {
        [field: SerializeField] public int PlankCount { get; set; }

        [SerializeField] private Collectible[] itemPrefabs;
        public Dictionary<int, Collectible> PoolObjectDic = new();

        /*[field: SerializeField] public float RangeX { get; set; }
        [field: SerializeField] public float RangeY { get; set; }*/

        [SerializeField] private Transform[] itemTransforms;

        private int poolIndex;

        private void Start()
        {
            PlankCount = BridgeManager.Instance.BridgeObjects.Length;
        }

        private void Update()
        {
            if (DayLoop.Instance.CurrentDayType == DayLoop.DayType.Day)
            {
                ItemSpawn();
            }
        }

        public void ItemSpawn()
        {
            /*poolIndex = 0;
            
            for (int i = 0; i < itemTransforms.Length; i++)
            {
                bool isExist = itemTransforms[i].childCount > 0;

                if (poolIndex < PlankCount && isExist == false)
                {
                    Collectible collectible = Instantiate(itemPrefabs[0], itemTransforms[poolIndex]);
                    PoolObjectDic.Add(poolIndex, collectible);
                    collectible.transform.position = itemTransforms[poolIndex].position;
                    poolIndex++;
                    Debug.Log(poolIndex);   
                }
                else if (poolIndex >= PlankCount && isExist == false)
                {
                    int rand = Random.Range(1, itemPrefabs.Length);
                    Collectible collectible = Instantiate(itemPrefabs[rand], itemTransforms[poolIndex]);
                    PoolObjectDic.Add(poolIndex, collectible);
                    collectible.transform.position = itemTransforms[poolIndex].position;
                    poolIndex++;
                    Debug.Log(poolIndex);
                    continue;
                }

                GameObject item = itemTransforms[i].GetChild(0).gameObject;
                
                if (item.activeSelf == false)
                {
                    Plank plank = item.GetComponent<Plank>();
                    
                    if (plank && plank.IsPickUp)
                    {
                        continue;
                    }
                    
                    item.SetActive(true);
                    item.transform.position = itemTransforms[i].position;
                }
            }*/
            
            for (int i = 0; i < itemTransforms.Length; i++)
            {
                Transform currentTransform = itemTransforms[i];
                bool isExist = currentTransform.childCount > 0;

                // 1. 아이템이 없는 경우 (새로 생성)
                if (!isExist)
                {
                    // i가 PlankCount보다 작으면 0번 프리팹, 아니면 랜덤 프리팹
                    int prefabIndex = (i < PlankCount) ? 0 : Random.Range(1, itemPrefabs.Length);
            
                    Collectible collectible = Instantiate(itemPrefabs[prefabIndex], currentTransform);
                    collectible.transform.position = currentTransform.position;
            
                    // Add 대신 인덱서를 사용해 중복 키 에러 방지
                    PoolObjectDic[i] = collectible; 
            
                    Debug.Log(i);
                    continue; // 생성을 완료했으므로 아래 재활용 로직은 건너뜀
                }

                // 2. 이미 아이템이 존재하는 경우 (비활성화 상태라면 재활용)
                GameObject item = currentTransform.GetChild(0).gameObject;
        
                if (!item.activeSelf)
                {
                    Plank plank = item.GetComponent<Plank>();
            
                    if (plank != null && plank.IsPickUp)
                    {
                        continue;
                    }
            
                    item.SetActive(true);
                    item.transform.position = currentTransform.position;
                }
            }
        }

        /*private Vector3 RandomTransform()
        {
            float minX = transform.position.x - RangeX;
            float maxX = transform.position.x + RangeX;

            float minY = transform.position.y - RangeY;
            float maxY = transform.position.y + RangeY;

            Vector3 returnVec = new Vector3(
                Random.Range(minX, maxX),
                Random.Range(minY, maxY),
                transform.position.z
            );

            return returnVec;
        }*/
    }
}