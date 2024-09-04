using Entity;
using Entity.Attack;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class HealthBar : MonoBehaviour
    {
        private Transform target;
        private HealthBarManager manager;
        private Camera mainCamera;
        private CharacterBase targetCharacter;
        private Slider slider;

        private void Awake()
        {
            mainCamera = Camera.main;
            slider = GetComponent<Slider>();
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
                Vector3 screenPos = mainCamera.WorldToScreenPoint(target.position + Vector3.up);
                
                // Check if the target is within the screen bounds
                if (screenPos.z > 0 && screenPos.x > 0 && screenPos.x < Screen.width && screenPos.y > 0 && screenPos.y < Screen.height)
                {
                    if (!targetCharacter)
                        targetCharacter = target.GetComponent<CharacterBase>();

                    transform.position = screenPos;
                    
                    if (targetCharacter)
                    {
                        var curHealth = targetCharacter.curHealth;
                        var maxHealth = targetCharacter.maxHealth;
                        slider.value = Mathf.Clamp((float)curHealth / maxHealth, 0f, 1f);
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