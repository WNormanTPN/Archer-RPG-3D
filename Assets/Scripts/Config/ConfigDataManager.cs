using System;
using System.Collections.Generic;
using System.Threading.Tasks;
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
            { ConfigType.Character, typeof(CharacterDataCollection) },
            { ConfigType.Effect, typeof(EffectCollection) },
            { ConfigType.Map, typeof(MapDataCollection) },
            { ConfigType.MapDetail, typeof(MapDetailDataCollection) },
            { ConfigType.MonsterWave, typeof(WaveDataCollection) },
            { ConfigType.Skill, typeof(SkillCollection) },
            { ConfigType.Weapon, typeof(WeaponCollection) },
        };

        public static ConfigDataManager Instance { get; private set; }

        private static TaskCompletionSource<bool> loadDataCompletionSource;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                loadDataCompletionSource = new TaskCompletionSource<bool>();
                _ = LoadAllDataAsync(); // Trigger async loading
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private Task LoadAllDataAsync()
        {
            foreach (var config in configFiles)
            {
                var json = config.file.text;
                var configType = config.type;
                var type = configTypeMap[configType];

                // Create an instance dynamically
                if (Activator.CreateInstance(type) is IConfigCollection collection)
                {
                    // Deserialize and cache the data
                    collection.FromJson(json);
                    cachedData[type] = collection;
                }
            }

            // Mark data loading as complete
            loadDataCompletionSource.SetResult(true);
            return Task.CompletedTask;
        }

        public T GetConfigData<T>() where T : class
        {
            // Wait until data loading is complete
            loadDataCompletionSource.Task.Wait();
            
            var type = typeof(T);
            if (cachedData.TryGetValue(type, out var data))
            {
                return data as T;
            }
            Debug.LogError($"No data found for type: {type}");
            return null;
        }
    }

    [Flags]
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
    
    public interface IConfigCollection
    {
        void FromJson(string json);
    }
}
