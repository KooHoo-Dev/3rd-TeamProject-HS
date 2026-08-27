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
        [SerializeField] private Collectible[] itemPrefabs;
        private PoolingSystem<Collectible>[] itemPools;

        /*[field: SerializeField] public float RangeX { get; set; }
        [field: SerializeField] public float RangeY { get; set; }*/

        [SerializeField] private Transform[] itemTransforms;

        private void Start()
        {
            itemPools = new PoolingSystem<Collectible>[itemPrefabs.Length];

            for (int i = 0; i < itemPools.Length; i++)
            {
                itemPools[i] = new PoolingSystem<Collectible>(itemPrefabs[i], itemTransforms[i]);
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
            for (int i = 0; i < itemTransforms.Length; i++)
            {
                bool isExist = itemTransforms[i].childCount > 0;
                Collectible item = itemPools[i].Get(isExist);
                item.Init(itemPools[i], itemTransforms[i].position);
                Plank plank = item.GetComponent<Plank>();

                if (plank != null && plank.IsPickUp)
                {
                    itemPools[i].Release(item);
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