using System.Collections.Generic;
using UnityEngine;

namespace Entity
{
    public class Effect : MonoBehaviour
    {
        public int effectID;
        public string effectName;
        public Dictionary<string, float> effectValues;
        public float duration;
        public GameObject effectPrefab;
    }
}
