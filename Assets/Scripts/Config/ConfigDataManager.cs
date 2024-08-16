using System;
using System.Collections.Generic;
using Entity;
using Evironment.MapGenerator;
using Newtonsoft.Json;
using UI;
using UnityEngine;

namespace Config
{
    public class ConfigDataManager : MonoBehaviour
    {
        [Serializable]
        public struct ConfigFile
        {
            public TextAsset file;
            public ConfigType type;
        }
        
        public List<ConfigFile> configFiles;

        private Dictionary<Type, object> cachedData = new Dictionary<Type, object>();
        private static readonly Dictionary<ConfigType, Type> configTypeMap = new Dictionary<ConfigType, Type>
        {
            { ConfigType.Character, typeof(Dictionary<string, CharacterData>) },
            { ConfigType.Effect, typeof(Dictionary<string, EffectData>) },
            { ConfigType.Map, typeof(MapCollection) },
            { ConfigType.MapDetail, typeof(Dictionary<string, MapDetail>) },
            { ConfigType.MonsterWave, typeof(Dictionary<string, List<WaveData>>) },
            { ConfigType.Skill, typeof(Dictionary<string, Skill>) },
            { ConfigType.Weapon, typeof(Dictionary<string, WeaponData>) },
        };

        public static ConfigDataManager Instance { get; private set; }

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                LoadAllData();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void LoadAllData()
        {
            foreach (var config in configFiles)
            {
                var json = config.file.text;
                var file = config.file;
                var configType = config.type;
                var type = configTypeMap[configType];
                var data = JsonConvert.DeserializeObject(json, type);
                cachedData[type] = data;
            }
        }

        public T GetConfigData<T>() where T : class
        {
            var type = typeof(T);
            if (cachedData.TryGetValue(type, out var data))
            {
                return data as T;
            }
            Debug.LogError($"No data found for type: {type}");
            return null;
        }
    }

    public enum ConfigType
    {
        Character,
        Effect,
        Map,
        MapDetail,
        MonsterWave,
        Skill,
        Weapon
    }
}
