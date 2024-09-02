using System;
using System.Collections.Generic;
using System.Linq;
using Config;
using Newtonsoft.Json;
using Unity.VisualScripting;
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
                    _effectCollection = new EffectCollection(ConfigDataManager.Instance.GetConfigData<EffectCollection>());
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
            var data = effectCollection.Effects[id.ToString()].DeepCopy();
            this.id = data.id;
            name = data.name;
            effectValues = data.effectValues;
            duration = data.duration;
            prefabKey = data.prefabKey;
        }
        
        public Effect(Effect effect)
        {
            var copied = effect.DeepCopy();
            id = copied.id;
            name = copied.name;
            effectValues = effect.effectValues;
            duration = copied.duration;
            prefabKey = copied.prefabKey;
        }
        
        public Effect DeepCopy()
        {
            string serializedObject = JsonConvert.SerializeObject(this);
            return JsonConvert.DeserializeObject<Effect>(serializedObject);
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
        
        public EffectCollection(EffectCollection collection)
        {
            var copied = collection.DeepCopy();
            Effects = copied.Effects;
        }

        public void FromJson(string json)
        {
            Effects = JsonConvert.DeserializeObject<Dictionary<string, Effect>>(json);
        }
        
        public EffectCollection DeepCopy()
        {
            string serializedObject = JsonConvert.SerializeObject(this);
            return JsonConvert.DeserializeObject<EffectCollection>(serializedObject);
        }
    }
}