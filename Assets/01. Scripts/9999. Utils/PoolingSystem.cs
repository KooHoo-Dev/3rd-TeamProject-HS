using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace WeGotOne
{
    public class PoolingSystem<T> where T : Component
    {
        private readonly List<T> objectPool = new();

        private readonly Transform parent;
        private readonly T original;
        
        private readonly Action<T> onGet;
        private readonly Action<T> onRelease;

        public PoolingSystem(T original, Transform parent = null,
            Action<T> onGet = null, Action<T> onRelease = null)
        {
            this.original = original;
            this.parent = parent;
            
            this.onGet = onGet ?? (item => item.gameObject.SetActive(true));
            this.onRelease = onRelease ?? (item => item.gameObject.SetActive(false));
        }
    
        public T Get(bool isExist)
        {
            if (isExist)
            {
                for (int i = 0; i < objectPool.Count; i++)
                {
                    if (objectPool[i].gameObject.activeSelf == false)
                    {
                        objectPool[i].gameObject.SetActive(true);
                        onGet(objectPool[i]);
                        return objectPool[i];
                    }
                }   
            }

            T newObject = Object.Instantiate(original, parent, true);
        
            objectPool.Add(newObject);
            newObject.gameObject.SetActive(true);
            onGet(newObject);
            return newObject;
        }
        
        public void Release(T item)
        {
            onRelease(item);
        }
    }
   
}