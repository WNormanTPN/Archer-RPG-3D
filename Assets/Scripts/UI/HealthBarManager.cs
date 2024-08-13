using System.Collections.Generic;
using Generic;
using UnityEngine;

namespace Entity.Attack
{
    public class HealthBarManager : MonoBehaviour
    {
        public GameObject healthBarPrefab; // Health bar prefab
        public Transform canvasTransform; // Canvas to parent health bars
        
        private ObjectPool objectPool; // Object pool for health bars
        private Dictionary<Transform, HealthBar> activeHealthBars = new Dictionary<Transform, HealthBar>();

        void Start()
        {
            objectPool = GameObject.FindGameObjectWithTag("ObjectPool").GetComponent<ObjectPool>();
        }

        public void RegisterTarget(Transform target)
        {
            if (!activeHealthBars.ContainsKey(target))
            {
                GameObject healthBarObj = objectPool.GetObject(healthBarPrefab);
                HealthBar healthBar = healthBarObj.GetComponent<HealthBar>();
                healthBar.Initialize(target, this);
                healthBarObj.transform.SetParent(canvasTransform, false);
                activeHealthBars.Add(target, healthBar);
            }
        }

        public void UnregisterTarget(Transform target)
        {
            if (activeHealthBars.TryGetValue(target, out HealthBar healthBar))
            {
                objectPool.ReturnObject(healthBarPrefab, healthBar.gameObject);
                activeHealthBars.Remove(target);
            }
        }

        void FixedUpdate()
        {
            foreach (var healthBar in activeHealthBars.Values)
            {
                healthBar.UpdateHealthBar();
            }
        }
    }
}