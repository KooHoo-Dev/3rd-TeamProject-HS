using System.Collections.Generic;
using UnityEngine;

namespace WeGotOne
{
    public class PoolingSystem
    {
        private readonly List<GameObject> objectPool = new List<GameObject>();

        private readonly Transform parent;
        private readonly GameObject original;

        public PoolingSystem(GameObject original, Transform parent = null)
        {
            this.original = original;
            this.parent = parent;
        }

        public GameObject ObjectPooling()
        {
            for (int i = 0; i < objectPool.Count; i++)
            {
                if (objectPool[i].activeSelf == false)
                {
                    objectPool[i].SetActive(true);
                    return objectPool[i];
                }
            }

            GameObject newObject = Object.Instantiate(original, parent.position, Quaternion.identity);

            objectPool.Add(newObject);
            newObject.SetActive(true);

            return newObject;
        }
    }
}