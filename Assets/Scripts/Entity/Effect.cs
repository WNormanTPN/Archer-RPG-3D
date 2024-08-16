using System;
using System.Collections.Generic;
using UnityEngine;

namespace Entity
{
    [Serializable]
    public class EffectData
    {
        public int id;
        public string name;
        public Dictionary<string, float> effectValues;
        public int duration;
        public string prefabKey;
    }
}