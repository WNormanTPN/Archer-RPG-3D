using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace Entity
{
    [Serializable]
    public class SkillData
    {
        public int skillID;
        public string skillName;
        public string skillIconKey;
        public List<int> effectIds;
        public Dictionary<string, float> attributes;
    }
    
    [Serializable]
    public class SkillDataCollection
    {
        public Dictionary<string, SkillData> skillDatas;
    }
}
