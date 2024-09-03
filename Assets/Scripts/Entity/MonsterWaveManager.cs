using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Config;
using Entity.Enemy;
using Evironment.MapGenerator;
using Generic;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Random = UnityEngine.Random;

namespace Entity
{
    public class MonsterWaveManager : MonoBehaviour
    {
        public GameObject player;                       // Assign your player prefab in the Inspector
        
        private List<WaveData> waveDatas;
        private ObjectPool objectPool;
        private int mapViewDistance;
        private Dictionary<string, GameObject> monsterPrefabs;
        private CharacterDataCollection monsterDatas;
        private MapGenerator mapGenerator;
        
        void Awake()
        {
            player = Instantiate(player);
            waveDatas = GetWaveData();
            objectPool = GameObject.FindGameObjectWithTag("ObjectPool").GetComponent<ObjectPool>();
            mapViewDistance = PlayerPrefs.GetInt("ViewDistance");
            monsterPrefabs = new Dictionary<string, GameObject>();
            mapGenerator = GameObject.FindGameObjectWithTag("MapGenerator").GetComponent<MapGenerator>();
            monsterDatas = ConfigDataManager.Instance.GetConfigData<CharacterDataCollection>();

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
                foreach (var monster in waveData.monsters)
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
                foreach (var monster in waveData.monsters)
                {
                    var monsterId = monster.Key;
                    var monsterCount = monster.Value;
                    for (int i = 0; i < monsterCount; i++)
                    {
                        var monsterData = GetMonsterData(monsterId);
                        var monsterPrefab = GetMonsterPrefab(monsterId);
                        var monsterObject = objectPool.GetObject(monsterPrefab);
                        var spawnPosition = new Vector3(Random.Range(spawnRangeX.x, spawnRangeX.y), 0, Random.Range(spawnRangeZ.x, spawnRangeZ.y));
                        monsterObject.GetComponent<EnemyController>().SetUpCharacter(monsterData, waveData.attack, waveData.maxHP);
                        monsterObject.transform.position = spawnPosition;
                        monsterObject.SetActive(true);
                    }
                }
                yield return new WaitForSeconds(waveData.nextTime);
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
        
        List<WaveData> GetWaveData()
        {
            var waveId = PlayerPrefs.GetInt("MonsterWaveGroup");
            return ConfigDataManager.Instance.GetConfigData<WaveDataCollection>().WaveDatas[waveId.ToString()];
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
        public Dictionary<string, List<WaveData>> WaveDatas;

        public WaveDataCollection()
        {
            WaveDatas = new Dictionary<string, List<WaveData>>();
        }
        
        [JsonConstructor]
        public WaveDataCollection(Dictionary<string, List<WaveData>> waveDatas)
        {
            this.WaveDatas = waveDatas;
        }

        public void FromJson(string json)
        {
            WaveDatas = JsonConvert.DeserializeObject<Dictionary<string, List<WaveData>>>(json);
        }
    }
}
