using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace Entity
{
    [Serializable]
    public class Skill
    {
        public int skillID;
        public string skillName;
        public string skillIconKey;
        public List<int> effectIds;
        public Dictionary<string, float> attributes;
    }
    
    [Serializable]
    public class SkillCollection
    {
        public Dictionary<string, Skill> Skills;
    }
}
