using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Config;
using Entity;
using Entity.Player;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class PlayerLevelUpManager : MonoBehaviour
    {
        public GameObject skillChoicePanel;
        
        private PlayerController playerController;
        private Dictionary<string, Skill> remaningSkills;
        private int remaningSkillCount;
        private List<SkillChoiceButton> skillChoiceButtons;
        
        private void CalculateRemainingSkills()
        {
            remaningSkills = ConfigDataManager.Instance.GetConfigData<SkillCollection>().Skills
                .ToDictionary(entry => entry.Key, entry => entry.Value);

            if (playerController.characterInitData?.skillIds == null) return;
            var curSkillIDs = playerController.characterInitData.skillIds;
            remaningSkillCount = 0;
            
            foreach (var skillID in curSkillIDs)
            {
                var curSkill = remaningSkills[skillID.ToString()];
                if (curSkill.currentStacks < curSkill.maxStacks)
                {
                    curSkill.currentStacks++;
                    remaningSkills[skillID.ToString()] = curSkill;
                }
                if (curSkill.currentStacks == curSkill.maxStacks)
                {
                    remaningSkills.Remove(skillID.ToString());
                }
            }
            
            foreach (var skill in remaningSkills)
            {
                remaningSkillCount += skill.Value.maxStacks - skill.Value.currentStacks;
            }
        }

        public void StartLevelUpProcess()
        {
            StartCoroutine(LevelUpCoroutine((result) =>
            {
                playerController.AddSkill(result);
            }));
        }

        public void Test()
        {
            Debug.Log("Test");
        }
        
        public IEnumerator LevelUpCoroutine(Action<int> onComplete)
        {
            if (playerController == null)
                playerController = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>();

            if (skillChoiceButtons == null)
                skillChoiceButtons = skillChoicePanel.GetComponentsInChildren<SkillChoiceButton>().ToList();

            if (remaningSkills == null)
            {
                CalculateRemainingSkills();
            }
            else if (remaningSkillCount == 0)
            {
                onComplete?.Invoke(-1);
                yield break;
            }

            CalculateRemainingSkills();
            var randomSkillIDs = GetRandomSkillIDs(3);
            skillChoicePanel.SetActive(true);
            int choicedSkillID = -1;

            // Clear previous listeners to avoid issues
            foreach (var button in skillChoiceButtons)
            {
                button.ClearListeners();
            }

            for (var i = 0; i < skillChoiceButtons.Count; i++)
            {
                var skill = remaningSkills[randomSkillIDs[i].ToString()];

                StartCoroutine(skillChoiceButtons[i].ShowSkillChoiceCoroutine(skill, skillID =>
                {
                    choicedSkillID = skillID;
                    ChooseSkill(choicedSkillID);
                    skillChoicePanel.SetActive(false);
                    onComplete.Invoke(choicedSkillID);
                }));
            }
        }

        private void ChooseSkill(int skillID)
        {
            var skill = remaningSkills[skillID.ToString()];
            skill.currentStacks++;
            remaningSkillCount--;
            if (skill.currentStacks == skill.maxStacks)
            {
                remaningSkills.Remove(skillID.ToString());
            }
            else
            {
                remaningSkills[skillID.ToString()] = skill;
            }
        }
        
        private List<int> GetRandomSkillIDs(int n)
        {
            var tempRemainingSkills = new Dictionary<string, Skill>(remaningSkills);
            var randomSkillIDs = new List<int>();
            var random = new System.Random();
            for (var i = 0; i < n; i++)
            {
                var randomIndex = random.Next(0, tempRemainingSkills.Count);
                var randomSkill = tempRemainingSkills.ElementAt(randomIndex).Value;
                randomSkillIDs.Add(randomSkill.skillID);
                if (randomSkill.currentStacks == randomSkill.maxStacks)
                {
                    tempRemainingSkills.Remove(randomSkill.skillID.ToString());
                }
                else
                {
                    randomSkill.currentStacks++;
                    tempRemainingSkills[randomSkill.skillID.ToString()] = randomSkill;
                }
            }
            return randomSkillIDs;
        }
    }
}
