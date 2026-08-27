using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.iOS;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

namespace WeGotOne
{
    public class ItemObjectPool : MonoBehaviour
    {
        [field: SerializeField] public int PlankCount { get; set; }

        [SerializeField] private Collectible[] itemPrefabs;
        private PoolingSystem<Collectible>[] itemPools;

        /*[field: SerializeField] public float RangeX { get; set; }
        [field: SerializeField] public float RangeY { get; set; }*/

        [SerializeField] private Transform[] itemTransforms;

        private int poolIndex;

        private void Start()
        {
            itemPools = new PoolingSystem<Collectible>[itemPrefabs.Length];
            PlankCount = BridgeManager.Instance.BridgeObjects.Length;

            for (int i = 0; i < itemPrefabs.Length; i++)
            {
                itemPools[i] = new PoolingSystem<Collectible>(itemPrefabs[i]);
            }

            ItemSpawn();
        }

        private void Update()
        {
            if (SimpleInput.GetKeyDown(Key.Digit1))
                ItemSpawn();
        }

        public void ItemSpawn()
        {
            poolIndex = 0;
            
            for (int i = 0; i < itemTransforms.Length; i++)
            {
                bool isExist = itemTransforms[i].childCount > 0;

                if (poolIndex < PlankCount && isExist == false)
                {
                    Collectible collectible = Instantiate(itemPrefabs[0], itemTransforms[poolIndex]);
                    collectible.transform.position = itemTransforms[poolIndex].position;
                    poolIndex++;
                    Debug.Log(poolIndex);   
                }
                else if (poolIndex >= PlankCount && isExist == false)
                {
                    int rand = Random.Range(1, itemPrefabs.Length);
                    Collectible collectible = Instantiate(itemPrefabs[rand], itemTransforms[poolIndex]);
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