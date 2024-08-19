using System;
using System.Collections.Generic;
using UnityEngine;

namespace Entity
{
    [Serializable]
    public class Effect
    {
        public int id;
        public string name;
        public Dictionary<string, float> effectValues;
        public int duration;
        public string prefabKey;
    }
    
    [Serializable]
    public class EffectCollection
    {
        public Dictionary<string, Effect> Effects;
    }
}