using System;
using System.Collections.Generic;
using Entity;
using Evironment.MapGenerator;
using Firebase;
using Firebase.Database;
using Firebase.Extensions;
using UI;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Config
{
    public class ConfigDataManager : MonoBehaviour
    {
        public bool active = false;
        [SerializeField] private GameObject loadingSlider;
        [SerializeField] private string mainMenuSceneKey;
        private Dictionary<Type, object> cachedData = new Dictionary<Type, object>();
        public static readonly Dictionary<ConfigType, Type> configTypeMap = new Dictionary<ConfigType, Type>
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

        private DatabaseReference databaseReference;
        private Slider slider;

        private void Start()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                if (active)
                {
                    slider = loadingSlider.GetComponent<Slider>();
                    InitializeFirebase();
                }
            }
            else
            {
                Destroy(gameObject);
            }
        }
        
        private void Update()
        {
            if (slider)
            {
                if (slider.value < 1)
                {
                    if (cachedData.Count == configTypeMap.Count)
                    {
                        slider.value += Time.deltaTime;
                    }
                    slider.value += 0.1f * Time.deltaTime;
                }
                else
                {
                    if (cachedData.Count < configTypeMap.Count)
                    {
                        slider.value = 0.8f;
                    }
                    else
                    {
                        Addressables.LoadSceneAsync(mainMenuSceneKey, LoadSceneMode.Single);
                        slider = null;
                    }
                }
            }
        }

        private void InitializeFirebase()
        {
            FirebaseApp.CheckAndFixDependenciesAsync()
                .ContinueWithOnMainThread(task => {
                if (task.Result == DependencyStatus.Available)
                {
                    databaseReference = FirebaseDatabase.DefaultInstance.RootReference;
                    if (databaseReference == null)
                    {
                        return;
                    }
                    Debug.Log("Firebase initialized.");
                    LoadAllData(); // Trigger async loading
                }
                else
                {
                    Debug.LogError($"Failed to initialize Firebase: {task.Result}");
                }
            });
        }


        private void LoadAllData()
        {
            foreach (var configType in configTypeMap.Keys)
            {
                var path = GetFirebasePathForType(configType);
                var type = configTypeMap[configType];
                
                var databaseRef = databaseReference.Child(path);
                
                if (configType != ConfigType.Map)
                    databaseRef = databaseRef.Child(path);

                databaseRef.GetValueAsync().ContinueWithOnMainThread((task) =>
                {
                    if (task.IsCompleted)
                    {
                        var json = task.Result.GetRawJsonValue();
                        var collection = Activator.CreateInstance(type) as IConfigCollection;
                        collection.FromJson(json);
                        cachedData[type] = collection;
                    }
                    else
                    {
                        Debug.LogError($"Failed to load data for type: {type}");
                    }
                });
            }
        }

        private string GetFirebasePathForType(ConfigType configType)
        {
            if (configType == ConfigType.Character) return "CharacterDatas";
            if (configType == ConfigType.Effect) return "Effects";
            if (configType == ConfigType.Map) return "Maps";
            if (configType == ConfigType.MapDetail) return "MapDetails";
            if (configType == ConfigType.MonsterWave) return "WaveDatas";
            if (configType == ConfigType.Skill) return "Skills";
            if (configType == ConfigType.Weapon) return "Weapons";
            return "unknown";
        }

        public T GetConfigData<T>() where T : class
        {
            if (cachedData.ContainsKey(typeof(T)))
            {
                return cachedData[typeof(T)] as T;
            }
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

    public interface IConfigCollection
    {
        void FromJson(string json);
    }
}
