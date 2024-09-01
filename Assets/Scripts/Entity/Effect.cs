using System;
using System.Collections.Generic;
using System.Linq;
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

        public static EffectCollection effectCollection
        {
            get
            {
                if (_effectCollection == null)
                {
                    _effectCollection = ConfigDataManager.Instance.GetConfigData<EffectCollection>();
                }

                return _effectCollection;
            }
            private set => _effectCollection = value;
        }

        private static EffectCollection _effectCollection;
        
        public Effect()
        {
            effectValues = new Dictionary<string, float>();
        }
        
        public Effect(int id)
        {
            var data = ConfigDataManager.Instance.GetConfigData<EffectCollection>().Effects[id.ToString()];
            this.id = data.id;
            name = data.name;
            effectValues = data.effectValues?.ToDictionary(entry => entry.Key, entry => entry.Value);
            duration = data.duration;
            prefabKey = data.prefabKey;
        }
    }
    
    [Serializable]
    public class EffectCollection : IConfigCollection
    {
        public Dictionary<string, Effect> Effects;

        public EffectCollection()
        {
            Effects = new Dictionary<string, Effect>();
        }
        
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