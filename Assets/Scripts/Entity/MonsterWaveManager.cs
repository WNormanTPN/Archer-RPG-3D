using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Config;
using Entity.Enemy;
using Evironment.MapGenerator;
using Generic;
using Newtonsoft.Json;
using UI;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Random = UnityEngine.Random;

namespace Entity
{
    public class MonsterWaveManager : MonoBehaviour
    {
        public GameObject player;                       // Assign your player prefab in the Inspector
        public Transform entitiesParent;                // Parent for all entities
        
        public static List<GameObject> monsters;        // List of alive monsters
        
        private Dictionary<string, WaveData> waveDatas;
        private ObjectPool objectPool;
        private int mapViewDistance;
        private Dictionary<string, GameObject> monsterPrefabs;
        private CharacterDataCollection monsterDatas;
        private MapGenerator mapGenerator;
        
        void Awake()
        {
            player = Instantiate(player);
            monsters = new List<GameObject>();
            waveDatas = GetWaveData();
            objectPool = GameObject.FindGameObjectWithTag("ObjectPool").GetComponent<ObjectPool>();
            mapViewDistance = PlayerPrefs.GetInt("ViewDistance");
            monsterPrefabs = new Dictionary<string, GameObject>();
            mapGenerator = GameObject.FindGameObjectWithTag("MapGenerator").GetComponent<MapGenerator>();
            monsterDatas = CharacterDataCollection.characterDataCollection;

            // Start the asynchronous setup
            SetUp();
        }

        async void SetUp()
        {
            // Load all needed assets
            List<Task<GameObject>> tasks = LoadAllNeededAssets();

            // Await the completion of all tasks
            await Task.WhenAll(tasks);

            StartCoroutine(SetUpWave());
        }


        List<Task<GameObject>> LoadAllNeededAssets()
        {
            List<Task<GameObject>> tasks = new();
            foreach (var waveData in waveDatas)
            {
                foreach (var monster in waveData.Value.monsters)
                {
                    var monsterId = monster.Key;
                    var task = LoadMonsterPrefab(monsterId);
                    if (task != null) tasks.Add(task);
                }
            }
            return tasks;
        }

        IEnumerator SetUpWave()
        {
            Vector2Int spawnRangeX = new Vector2Int(-mapViewDistance, mapViewDistance);
            Vector2Int spawnRangeZ = new Vector2Int(-mapViewDistance, mapViewDistance);
            foreach (var waveData in waveDatas)
            {
                if (!mapGenerator.isLimitedMap)
                {
                    var playerPosition = new Vector2Int((int)player.transform.position.x, (int)player.transform.position.z);
                    spawnRangeX += Vector2Int.one * playerPosition.x;
                    spawnRangeZ += Vector2Int.one * playerPosition.y;
                }
                foreach (var monster in waveData.Value.monsters)
                {
                    var monsterId = monster.Key;
                    var monsterCount = monster.Value;
                    for (int i = 0; i < monsterCount; i++)
                    {
                        var monsterData = GetMonsterData(monsterId);
                        var monsterPrefab = GetMonsterPrefab(monsterId);
                        var monsterObject = objectPool.GetObject(monsterPrefab, entitiesParent);
                        var spawnPosition = new Vector3(
                            Random.Range(spawnRangeX.x, spawnRangeX.y),
                            0,
                            Random.Range(spawnRangeZ.x, spawnRangeZ.y)
                            );
                        monsterObject.GetComponent<EnemyController>()
                            .SetUpCharacter(monsterData, waveData.Value.attack, waveData.Value.maxHP);
                        monsterObject.transform.position = spawnPosition;
                        monsterObject.SetActive(true);
                        monsters.Add(monsterObject);
                    }
                }
                yield return new WaitForSeconds(waveData.Value.nextTime);
            }
        }
        
        Task<GameObject> LoadMonsterPrefab(string monsterId)
        {
            if (monsterPrefabs.ContainsKey(monsterId)) return null;
            monsterPrefabs.Add(monsterId, null);
            var monsterData = GetMonsterData(monsterId);
            var loadAssetAsync = Addressables.LoadAssetAsync<GameObject>(monsterData.prefabKey);
            loadAssetAsync.Completed += handle =>
            {
                monsterPrefabs[monsterId] = handle.Result;
            };
            return loadAssetAsync.Task;
        }
        
        GameObject GetMonsterPrefab(string monsterId)
        {
            return monsterPrefabs[monsterId];
        }
        
        CharacterData GetMonsterData(string monsterId)
        {
            return monsterDatas.CharacterDatas[monsterId];
        }
        
        Dictionary<string, WaveData> GetWaveData()
        {
            var waveId = PlayerPrefs.GetInt("MonsterWaveGroup");
            return WaveDataCollection.waveDataCollection.WaveDatas[waveId.ToString()];
        }
    }
    
    [Serializable]
    public class WaveData
    {
        public int waveID;
        public int waveGroup;
        public int nextTime;
        public float attack;
        public float maxHP;
        public Dictionary<string, int> monsters;
    }
    
    [Serializable]
    public class WaveDataCollection : IConfigCollection
    {
        public Dictionary<string, Dictionary<string, WaveData>> WaveDatas;
        public static WaveDataCollection waveDataCollection
        {
            get
            {
                if (_waveDataCollection == null)
                {
                    _waveDataCollection = new WaveDataCollection(ConfigDataManager.Instance.GetConfigData<WaveDataCollection>());
                }

                return _waveDataCollection;
            }
            private set => _waveDataCollection = value;
        }
        private static WaveDataCollection _waveDataCollection;

        public WaveDataCollection()
        {
            WaveDatas = new Dictionary<string, Dictionary<string, WaveData>>();
        }
        
        [JsonConstructor]
        public WaveDataCollection(Dictionary<string, Dictionary<string, WaveData>> waveDatas)
        {
            this.WaveDatas = waveDatas;
        }

        public WaveDataCollection(WaveDataCollection data)
        {
            var copied = data.DeepCopy();
            WaveDatas = copied.WaveDatas;
        }

        public void FromJson(string json)
        {
            WaveDatas = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, WaveData>>>(json);
        }
        
        public WaveDataCollection DeepCopy()
        {
            string serializedObject = JsonConvert.SerializeObject(this);
            return JsonConvert.DeserializeObject<WaveDataCollection>(serializedObject);
        }
    }
}
