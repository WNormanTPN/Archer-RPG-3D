using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Entity;
using Entity.Player;
using UnityEngine;

namespace UI
{
    public class PlayerLevelUpManager : MonoBehaviour
    {
        public GameObject skillChoicePanel;
        
        private PlayerController playerController;
        private SkillCollection remaningSkills;
        private int remaningSkillCount;
        private List<SkillChoiceButton> skillChoiceButtons;
        int needChoice = 0;

        private void CalculateRemainingSkills()
        {
            remaningSkills = new SkillCollection(Skill.skillCollection);
            remaningSkillCount = 0;
            var curSkills = playerController.skills.Skills;
            
            foreach (var curSkill in curSkills.Values)
            {
                if (curSkill.currentStacks < curSkill.maxStacks)
                {
                    remaningSkills.Skills[curSkill.skillID.ToString()].currentStacks = curSkill.currentStacks;
                }
                if (curSkill.currentStacks == curSkill.maxStacks)
                {
                    remaningSkills.Skills.Remove(curSkill.skillID.ToString());
                }
                
                // foreach (var exclusion in curSkill.exclusions)
                // {
                //     if (remaningSkills.Skills.ContainsKey(exclusion.ToString()))
                //     {
                //         remaningSkills.Skills.Remove(exclusion.ToString());
                //     }
                // }
            }
            
            foreach (var skill in remaningSkills.Skills.Values)
            {
                remaningSkillCount += skill.maxStacks - skill.currentStacks;
            }
        }

        public void StartLevelUpProcess()
        {
            if (needChoice == 0)
            {
                needChoice++;
                StartLevelupProcessHelper();
            }
            else
            {
                needChoice++;
            }
        }
        
        private void StartLevelupProcessHelper()
        {
            StartCoroutine(LevelUpCoroutine((result) =>
            {
                if (result == -1) return;
                playerController.AddSkill(result);
                needChoice--;
                
                if (needChoice > 0)
                {
                    StartLevelupProcessHelper();
                }
            }));
        }

        
        public IEnumerator LevelUpCoroutine(Action<int> onComplete)
        {
            if (!playerController)
                playerController = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>();

            if (skillChoiceButtons == null || skillChoiceButtons.Count == 0)
                skillChoiceButtons = skillChoicePanel.GetComponentsInChildren<SkillChoiceButton>().ToList();


            CalculateRemainingSkills();
            if (remaningSkillCount == 0)
            {
                onComplete?.Invoke(-1);
                yield break;
            }
            
            var randomSkillIDs = GetRandomSkillIDs(3);
            skillChoicePanel.SetActive(true);
            int choicedSkillID = -1;
            
            // Clear previous listeners to avoid issues
            foreach (var button in skillChoiceButtons)
            {
                button.ClearListeners();
            }

            for (int i = 0; i < skillChoiceButtons.Count; i++)
            {
                skillChoiceButtons[i].gameObject.SetActive(true);
            }
            
            for (var i = 0; i < skillChoiceButtons.Count; i++)
            {
                if (randomSkillIDs.Count <= i)
                {
                    StartCoroutine(skillChoiceButtons[i].ShowSkillChoiceCoroutine(null, skillID =>
                    {
                        choicedSkillID = skillID;
                        skillChoicePanel.SetActive(false);
                        onComplete.Invoke(choicedSkillID);
                    }));
                    continue;
                }
                var skill = new Skill(remaningSkills.Skills[randomSkillIDs[i].ToString()]);
                
                StartCoroutine(skillChoiceButtons[i].ShowSkillChoiceCoroutine(skill, skillID =>
                {
                    choicedSkillID = skillID;
                    skillChoicePanel.SetActive(false);
                    onComplete.Invoke(choicedSkillID);
                }));
            }
        }
        
        private List<int> GetRandomSkillIDs(int n)
        {
            var tempRemainingSkills = new SkillCollection(remaningSkills);
            var randomSkillIDs = new List<int>();
            var random = new System.Random();
            for (var i = 0; i < n; i++)
            {
                if (tempRemainingSkills.Skills.Count == 0) break;
                var randomIndex = random.Next(0, tempRemainingSkills.Skills.Count);
                var randomSkill = tempRemainingSkills.Skills.ElementAt(randomIndex).Value;
                randomSkill.currentStacks++;
                randomSkillIDs.Add(randomSkill.skillID);
                if (randomSkill.currentStacks == randomSkill.maxStacks)
                {
                    tempRemainingSkills.Skills.Remove(randomSkill.skillID.ToString());
                }
            }
            return randomSkillIDs;
        }
    }
}
