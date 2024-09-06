using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Entity;
using Evironment.MapGenerator;
using Firebase;
using Newtonsoft.Json;
using UI;
using UnityEngine;
using Firebase.Database;
using Firebase.Extensions;

namespace Config
{
    public class SaveDataToFirebase : MonoBehaviour
    {
        public bool active = false;
        [Serializable]
        public struct ConfigFile
        {
            public ConfigType type;
            public TextAsset file;
        }
        
        public List<ConfigFile> configFiles;

        private Dictionary<Type, object> cachedData = new Dictionary<Type, object>();
        private static TaskCompletionSource<bool> saveDataCompletionSource;
        private DatabaseReference databaseReference;

        private void Awake()
        {
            if (active)
            {
                saveDataCompletionSource = new TaskCompletionSource<bool>();
                InitializeFirebase(); // Initialize Firebase
            }
        }

        
        private void InitializeFirebase()
        {
            FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task => {
                if (task.Result == DependencyStatus.Available)
                {
                    FirebaseApp app = FirebaseApp.DefaultInstance;
                    databaseReference = FirebaseDatabase.DefaultInstance.RootReference;
                    _ = SaveAllDataAsync(); // Trigger async saving
                }
                else
                {
                    Debug.LogError($"Failed to initialize Firebase: {task.Result}");
                }
            });
        }

        private async Task SaveDataToFirebaseAsync()
        {
            foreach (var kvp in cachedData)
            {
                var type = kvp.Key;
                var data = kvp.Value;
                var json = JsonConvert.SerializeObject(data);
                var path = GetFirebasePathForType(type);

                try
                {
                    if (databaseReference != null)
                    {
                        await databaseReference.Child(path).SetRawJsonValueAsync(json);
                    }
                    else
                    {
                        Debug.LogError("Database reference is not initialized.");
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Failed to upload data for type: {type}. Error: {ex.Message}");
                }
            }
            Debug.Log("All data saved to Firebase.");
        }

        private async Task SaveAllDataAsync()
        {
            foreach (var config in configFiles)
            {
                var json = config.file.text;
                var configType = config.type;
                var type = ConfigDataManager.configTypeMap[configType];

                if (Activator.CreateInstance(type) is IConfigCollection collection)
                {
                    collection.FromJson(json);
                    cachedData[type] = collection;
                }
            }

            await SaveDataToFirebaseAsync(); // Save data to Firebase
            saveDataCompletionSource.SetResult(true);
        }

        
        private static string GetFirebasePathForType(Type type)
        {
            // Determine the path in Firebase based on the type
            // This should match the structure you want in Firebase
            if (type == typeof(CharacterDataCollection)) return "CharacterDatas";
            if (type == typeof(EffectCollection)) return "Effects";
            if (type == typeof(MapDataCollection)) return "Maps";
            if (type == typeof(MapDetailDataCollection)) return "MapDetails";
            if (type == typeof(WaveDataCollection)) return "WaveDatas";
            if (type == typeof(SkillCollection)) return "Skills";
            if (type == typeof(WeaponCollection)) return "Weapons";
            return "unknown";
        }
    }
}
