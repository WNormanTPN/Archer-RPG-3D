using System;
using System.Collections.Generic;
using System.Linq;
using Config;
using Newtonsoft.Json;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace Entity
{
    [Serializable]
    public class Skill
    {
        public int skillID;
        public string name;
        public string skillIcon;
        [JsonProperty("effects")]public List<int> effectIDs;
        [JsonProperty("max")] public int maxStacks;
        public int currentStacks = 0;
        public Dictionary<string, float> attributes;
        public List<int> exclusions;
        
        
        public Skill()
        {
            effectIDs = new List<int>();
            attributes = new Dictionary<string, float>();
            exclusions = new List<int>();
        }

        public Skill(int skillID)
        {
            var data = ConfigDataManager.Instance.GetConfigData<SkillCollection>().Skills[skillID.ToString()];
            this.skillID = data.skillID;
            name = data.name;
            skillIcon = data.skillIcon;
            effectIDs = data.effectIDs?.ToList();
            maxStacks = data.maxStacks;
            attributes = data.attributes?.ToDictionary(entry => entry.Key, entry => entry.Value);;
            exclusions = data.exclusions?.ToList();
        }
    }
    
    [Serializable]
    public class SkillCollection : IConfigCollection
    {
        public Dictionary<string, Skill> Skills;

        public SkillCollection()
        {
            Skills = new Dictionary<string, Skill>();
        }
        
        [JsonConstructor]
        public SkillCollection(Dictionary<string, Skill> Skills)
        {
            this.Skills = Skills;
        }

        public void FromJson(string json)
        {
            Skills = JsonConvert.DeserializeObject<Dictionary<string, Skill>>(json);
        }
        
        public Skill AddSkill(Skill skill)
        {
            if (Skills.ContainsKey(skill.skillID.ToString()))
            {
                var curSkill = Skills[skill.skillID.ToString()];
                if (curSkill.currentStacks < curSkill.maxStacks)
                {
                    List<KeyValuePair<string, float>> attributesToAdd = new List<KeyValuePair<string, float>>();

                    foreach (KeyValuePair<string, float> kvp in skill.attributes)
                    {
                        attributesToAdd.Add(kvp);
                    }

                    foreach (var kvp in attributesToAdd)
                    {
                        curSkill.attributes[kvp.Key] += kvp.Value;
                    }

                    curSkill.currentStacks++;

                    RemoveAllExclusions(skill);
                }
                return curSkill;
            }

            Skills[skill.skillID.ToString()] = skill;
            skill.currentStacks++;
            RemoveAllExclusions(skill);
            
            return skill;
        }


        private void RemoveAllExclusions(Skill skill)
        {
            List<string> keysToRemove = new List<string>();

            foreach (var exclusion in skill.exclusions)
            {
                keysToRemove.Add(exclusion.ToString());
            }

            foreach (var key in keysToRemove)
            {
                Skills.Remove(key);
            }
        }
    }
}
