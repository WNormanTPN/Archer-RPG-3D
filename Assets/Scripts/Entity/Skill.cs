using System;
using System.Collections.Generic;
using System.Linq;
using Config;
using Newtonsoft.Json;
using Unity.VisualScripting;
using UnityEngine;

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
        
        public static SkillCollection skillCollection
        {
            get
            {
                if (_skillCollection == null)
                {
                    _skillCollection = new SkillCollection(ConfigDataManager.Instance.GetConfigData<SkillCollection>());
                }

                return _skillCollection;
            }
            private set => _skillCollection = value;
        }
        private static SkillCollection _skillCollection;
        
        
        public Skill()
        {
            effectIDs = new List<int>();
            attributes = new Dictionary<string, float>();
            exclusions = new List<int>();
        }

        public Skill(int skillID)
        {
            var data = skillCollection.Skills[skillID.ToString()].DeepCopy();
            this.skillID = data.skillID;
            name = data.name;
            skillIcon = data.skillIcon;
            effectIDs = data.effectIDs;
            maxStacks = data.maxStacks;
            attributes = data.attributes;
            exclusions = data.exclusions;
        }
        
        public Skill(Skill skill)
        {
            var copied = skill.DeepCopy();
            skillID = copied.skillID;
            name = copied.name;
            skillIcon = copied.skillIcon;
            effectIDs = copied.effectIDs;
            maxStacks = copied.maxStacks;
            currentStacks = copied.currentStacks;
            attributes = copied.attributes;
            exclusions =copied.exclusions;
        }
        
        public Skill DeepCopy()
        {
            string serializedObject = JsonConvert.SerializeObject(this);
            return JsonConvert.DeserializeObject<Skill>(serializedObject);
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
        
        public SkillCollection(SkillCollection skillCollection)
        {
            var copied = skillCollection.DeepCopy();
            Skills = copied.Skills;
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
                    foreach (KeyValuePair<string, float> kvp in skill.attributes)
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
        
        public SkillCollection DeepCopy()
        {
            string serializedObject = JsonConvert.SerializeObject(this);
            return JsonConvert.DeserializeObject<SkillCollection>(serializedObject);
        }
    }
}
