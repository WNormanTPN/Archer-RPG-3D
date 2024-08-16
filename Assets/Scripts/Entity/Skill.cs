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
        public struct SkillAttribute
        {
            string attributeName;
            float attributeValue;
        }
        public int skillID;
        public string skillName;
        public string skillIconKey;
        public List<int> effectIds;
        public List<SkillAttribute> attributes;
    }
}
