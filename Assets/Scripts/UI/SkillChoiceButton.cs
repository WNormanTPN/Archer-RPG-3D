using System.Collections;
using System.Threading.Tasks;
using Entity;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;

namespace UI
{
    public class SkillChoiceButton : MonoBehaviour
    {
        public Image icon;
        public TextMeshProUGUI skillName;

        private Button button;
        private TaskCompletionSource<int> tcs;
        private int skillID = -1;
        private System.Action<int> onChoice;

        private void Awake()
        {
            button = GetComponent<Button>();
        }

        public IEnumerator ShowSkillChoiceCoroutine(Skill skill, System.Action<int> onChoice)
        {
            ClearListeners();
            if (skill == null)
            {
                this.gameObject.SetActive(false);
                yield break;
            }
            var skillTexture = Addressables.LoadAssetAsync<Texture2D>(skill.skillIcon);
            skillTexture.WaitForCompletion();
        
            if (skillTexture.Status != AsyncOperationStatus.Succeeded)
            {
                Debug.LogError("Failed to load skill texture.");
                yield break;
            }

            var skillIcon = Sprite.Create(skillTexture.Result, new Rect(0, 0, skillTexture.Result.width, skillTexture.Result.height), Vector2.zero);
            icon.sprite = skillIcon;
            skillName.text = skill.name;
            tcs = new TaskCompletionSource<int>();
            skillID = skill.skillID;
            this.onChoice = onChoice;
            button.onClick.AddListener(OnButtonClick);
        }

        private void OnButtonClick()
        {
            // Check if TaskCompletionSource is not null and not completed
            if (tcs != null && !tcs.Task.IsCompleted)
            {
                tcs.SetResult(skillID);
                onChoice.Invoke(skillID);
            }
        }

        // Clear previous listeners to avoid multiple calls
        public void ClearListeners()
        {
            button.onClick.RemoveAllListeners();
        }
    }
}