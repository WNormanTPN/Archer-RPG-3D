using Entity;
using Entity.Attack;
using UnityEngine;

namespace UI
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

                    var targetCharacter = target.GetComponent<CharacterBase>();
                    if (targetCharacter)
                    {
                        var curHealth = targetCharacter.curHealth;
                        var maxHealth = targetCharacter.maxHealth;
                        var slider = GetComponent<UnityEngine.UI.Slider>();
                        slider.value = Mathf.Clamp(curHealth / maxHealth, 0, 1);
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