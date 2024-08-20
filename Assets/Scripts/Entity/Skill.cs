using System;
using System.Collections.Generic;
using Config;
using Newtonsoft.Json;
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
        public Dictionary<string, float> attributes;
    }
    
    [Serializable]
    public class SkillCollection : IConfigCollection
    {
        public Dictionary<string, Skill> Skills;
        
        public SkillCollection() {}
        
        [JsonConstructor]
        public SkillCollection(Dictionary<string, Skill> Skills)
        {
            this.Skills = Skills;
        }

        public void FromJson(string json)
        {
            Skills = JsonConvert.DeserializeObject<Dictionary<string, Skill>>(json);
        }
    }
}
