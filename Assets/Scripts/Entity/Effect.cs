using System;
using System.Collections.Generic;
using Config;
using Newtonsoft.Json;
using UnityEngine;

namespace Entity
{
    [Serializable]
    public class Effect
    {
        [JsonProperty("effectID")]
        public int id;
        
        public string name;
        
        [JsonProperty("changeValue")]
        public Dictionary<string, float> effectValues;
        
        public int duration;
        
        [JsonProperty("effect")]
        public string prefabKey;
    }
    
    [Serializable]
    public class EffectCollection : IConfigCollection
    {
        public Dictionary<string, Effect> Effects;
        
        public EffectCollection() {}
        
        [JsonConstructor]
        public EffectCollection(Dictionary<string, Effect> Effects)
        {
            this.Effects = Effects;
        }

        public void FromJson(string json)
        {
            Effects = JsonConvert.DeserializeObject<Dictionary<string, Effect>>(json);
        }
    }
}