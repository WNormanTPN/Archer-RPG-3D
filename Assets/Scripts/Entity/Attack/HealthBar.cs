using System;
using UnityEngine;

namespace Entity.Attack
{
    public class HealthBar : MonoBehaviour
    {
        private Transform target;
        private HealthBarManager manager;
        private Camera mainCamera;

        private void Awake()
        {
            mainCamera = Camera.main;
        }

        public void Initialize(Transform targetTransform, HealthBarManager manager)
        {
            this.target = targetTransform;
            this.manager = manager;
            gameObject.SetActive(true);
        }

        public void UpdateHealthBar()
        {
            if (target)
            {
                Vector3 screenPos = mainCamera.WorldToScreenPoint(target.position + Vector3.up * 2f);
                
                // Check if the target is within the screen bounds
                if (screenPos.z > 0 && screenPos.x > 0 && screenPos.x < Screen.width && screenPos.y > 0 && screenPos.y < Screen.height)
                {
                    transform.position = screenPos;
                    
                    var iHealth = target.GetComponent<IHealth>();
                    if (iHealth != null)
                    {
                        var curHealth = iHealth.curHealth;
                        var maxHealth = iHealth.maxHealth;
                        var slider = GetComponent<UnityEngine.UI.Slider>();
                        slider.value = curHealth / maxHealth;
                    }
                }
                else
                {
                    manager.UnregisterTarget(target);
                }
            }
            else
            {
                manager.UnregisterTarget(target);
            }
        }
    }
}