using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Entity
{
    public class Skill : MonoBehaviour
    {
        public struct SkillAttribute
        {
            string attributeName;
            float attributeValue;
        }
        public int skillID;
        public string skillName;
        public Image skillIcon;
        public List<Effect> effects;
        public List<SkillAttribute> attributes;
    }
}
