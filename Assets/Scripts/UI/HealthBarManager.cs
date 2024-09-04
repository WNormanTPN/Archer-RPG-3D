using System.Collections.Generic;
using Generic;
using UnityEngine;

namespace UI
{
    public class HealthBarManager : MonoBehaviour
    {
        public GameObject healthBarPrefab; // Health bar prefab
        public Transform canvasTransform; // Canvas to parent health bars
        
        private ObjectPool objectPool; // Object pool for health bars
        private Dictionary<Transform, HealthBar> activeHealthBars = new Dictionary<Transform, HealthBar>();
        private List<Transform> targetsToRemove = new List<Transform>(); // List to store targets that need removal

        void Start()
        {
            objectPool = GameObject.FindGameObjectWithTag("ObjectPool").GetComponent<ObjectPool>();
        }

        public void RegisterTarget(Transform target)
        {
            if (!activeHealthBars.ContainsKey(target))
            {
                GameObject healthBarObj = objectPool.GetObject(healthBarPrefab, canvasTransform);
                HealthBar healthBar = healthBarObj.GetComponent<HealthBar>();
                healthBar.Initialize(target, this);
                healthBarObj.transform.SetParent(canvasTransform, false);
                healthBarObj.SetActive(true);
                activeHealthBars.Add(target, healthBar);
            }
        }

        public void UnregisterTarget(Transform target)
        {
            if (activeHealthBars.ContainsKey(target))
            {
                targetsToRemove.Add(target); // Mark the target for removal
            }
        }

        void FixedUpdate()
        {
            // Update health bars
            foreach (var healthBar in activeHealthBars.Values)
            {
                healthBar.UpdateHealthBar();
            }

            // Remove health bars after update
            foreach (var target in targetsToRemove)
            {
                if (activeHealthBars.TryGetValue(target, out HealthBar healthBar))
                {
                    objectPool.ReturnObject(healthBarPrefab, healthBar.gameObject);
                    activeHealthBars.Remove(target);
                }
            }

            targetsToRemove.Clear(); // Clear the list after removals
        }
    }
}
