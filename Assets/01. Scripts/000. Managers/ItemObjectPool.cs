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
        [field: SerializeField] public int[] MaxItemCounts { get; set; }

        [SerializeField] private Collectible[] itemPrefabs;
        private GameObjectPool<Collectible>[] itemPools;

        /*[field: SerializeField] public float RangeX { get; set; }
        [field: SerializeField] public float RangeY { get; set; }*/

        [SerializeField] private Transform[] plankTransforms;
        [SerializeField] private Transform[] fishTransforms;

        private void Start()
        {
            itemPools = new GameObjectPool<Collectible>[itemPrefabs.Length];
            
            for (int i = 0; i < itemPrefabs.Length; i++)
            {
                itemPools[i] = new GameObjectPool<Collectible>(itemPrefabs[i]);
            }

            ItemSpawn();
        }

        private void Update()
        {
            if(SimpleInput.GetKeyDown(Key.Digit1))
                ItemSpawn();
        }

        public void ItemSpawn()
        {
            int plankIndex = 0;
            int fishIndex = 0;
            
            for (int i = 0; i < itemPrefabs.Length; i++)
            {
                ItemType currentType = itemPrefabs[i].ThisItem.ItemType;

                for (int j = 0; j < MaxItemCounts[i]; j++)
                {
                    if (currentType == ItemType.Plank)
                    {
                        if (plankIndex < plankTransforms.Length)
                        {
                            Collectible item = itemPools[i].Get();
                            Plank plank = item.GetComponent<Plank>();
                            
                            if (plank != null && plank.IsPickUp)
                            {
                                itemPools[i].Release(item); 
                                continue;
                            }
                            
                            item.Init(itemPools[i], plankTransforms[plankIndex++].position);
                        }
                    }
                    else if (currentType == ItemType.Fish)
                    {
                        if (fishIndex < fishTransforms.Length)
                        {
                            Collectible item = itemPools[i].Get();
                            item.Init(itemPools[i], fishTransforms[fishIndex++].position);
                        }
                    }
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