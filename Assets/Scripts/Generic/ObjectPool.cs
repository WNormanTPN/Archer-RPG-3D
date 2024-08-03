using System.Collections.Generic;
using UnityEngine;

namespace Generic
{
    public class ObjectPool : MonoBehaviour
    {
        [System.Serializable]
        public struct Pool
        {
            public GameObject prefab;
            public Transform parent;
            public int initialPoolSize;
        }

        public Pool[] pools;
        private Dictionary<GameObject, Queue<GameObject>> poolDictionary;

        void Awake()
        {
            InitializePoolDictionary();
        }

        private void InitializePoolDictionary()
        {
            poolDictionary = new Dictionary<GameObject, Queue<GameObject>>(pools.Length);

            foreach (var pool in pools)
            {
                Queue<GameObject> objectPool = new Queue<GameObject>(pool.initialPoolSize);

                for (int i = 0; i < pool.initialPoolSize; i++)
                {
                    GameObject obj = Instantiate(pool.prefab, pool.parent);
                    obj.SetActive(false);
                    objectPool.Enqueue(obj);
                }
                poolDictionary.Add(pool.prefab, objectPool);
            }
        }

        public GameObject GetObject(GameObject prefab)
        {
            if (!poolDictionary.TryGetValue(prefab, out _))
            {
                poolDictionary.Add(prefab, new Queue<GameObject>());
            }
        
            var objectPool = poolDictionary[prefab];
            if (objectPool.Count > 0)
            {
                var obj = objectPool.Dequeue();
                obj.SetActive(true);
                return obj;
            }
        
            var newObj = Instantiate(prefab);
            newObj.SetActive(true);
            return newObj;
        }
        
        public GameObject GetObject(GameObject prefab, Transform parent)
        {
            if (!poolDictionary.TryGetValue(prefab, out _))
            {
                poolDictionary.Add(prefab, new Queue<GameObject>());
            }
        
            var objectPool = poolDictionary[prefab];
            if (objectPool.Count > 0)
            {
                var obj = objectPool.Dequeue();
                obj.SetActive(true);
                return obj;
            }
        
            var newObj = Instantiate(prefab, parent);
            newObj.SetActive(true);
            return newObj;
        }

        public void ReturnObject(GameObject prefab, GameObject obj)
        {
            obj.SetActive(false);
            poolDictionary[prefab].Enqueue(obj);    
        }
    }
}