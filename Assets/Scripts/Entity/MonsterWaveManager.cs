using System;
using System.Collections;
using System.Collections.Generic;
using Config;
using Evironment.MapGenerator;
using Generic;
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
        private Dictionary<int, GameObject> monsterPrefabs;
        private CharacterDataCollection monsterDatas;
        private MapGenerator mapGenerator;
        
        void Awake()
        {
            player = Instantiate(player);
            waveDatas = GetWaveData();
            objectPool = GameObject.FindGameObjectWithTag("ObjectPool").GetComponent<ObjectPool>();
            mapViewDistance = PlayerPrefs.GetInt("ViewDistance");
            monsterPrefabs = new Dictionary<int, GameObject>();
            monsterDatas = ConfigDataManager.Instance.GetConfigData<CharacterDataCollection>();
            LoadAllNeededAssets();
        }

        private void Start()
        {
            mapGenerator = GameObject.FindGameObjectWithTag("MapGenerator").GetComponent<MapGenerator>();
            StartCoroutine(SetUpWave());
        }
        
        void LoadAllNeededAssets()
        {
            foreach (var waveData in waveDatas)
            {
                foreach (var monster in waveData.monsters)
                {
                    var monsterId = monster[0];
                    LoadMonsterPrefab(monsterId);
                }
            }
        }

        IEnumerator SetUpWave()
        {
            Vector2Int spawnRangeX = new Vector2Int(-mapViewDistance, mapViewDistance);
            Vector2Int spawnRangeZ = new Vector2Int(-mapViewDistance, mapViewDistance);
            foreach (var waveData in waveDatas)
            {
                yield return new WaitForSeconds(waveData.nextTime);
                if (!mapGenerator.isLimitedMap)
                {
                    var playerPosition = new Vector2Int((int)player.transform.position.x, (int)player.transform.position.z);
                    spawnRangeX += Vector2Int.one * playerPosition.x;
                    spawnRangeZ += Vector2Int.one * playerPosition.y;
                }
                foreach (var monster in waveData.monsters)
                {
                    var monsterId = monster[0];
                    var monsterCount = monster[1];
                    for (int i = 0; i < monsterCount; i++)
                    {
                        var monsterData = GetMonsterData(monsterId);
                        var monsterPrefab = GetMonsterPrefab(monsterId);
                        var monsterObject = objectPool.GetObject(monsterPrefab);
                        var spawnPosition = new Vector3(Random.Range(spawnRangeX.x, spawnRangeX.y), 0, Random.Range(spawnRangeZ.x, spawnRangeZ.y));
                        monsterObject.transform.position = spawnPosition;
                        monsterObject.SetActive(true);
                    }
                }
            }
        }
        
        void LoadMonsterPrefab(int monsterId)
        {
            if (monsterPrefabs.ContainsKey(monsterId)) return;
            monsterPrefabs.Add(monsterId, null);
            var monsterData = GetMonsterData(monsterId);
            var loadAssetAsync = Addressables.LoadAssetAsync<GameObject>(monsterData.prefabKey);
            loadAssetAsync.Completed += handle =>
            {
                monsterPrefabs[monsterId] = handle.Result;
            };
        }
        
        GameObject GetMonsterPrefab(int monsterId)
        {
            return monsterPrefabs[monsterId];
        }
        
        CharacterData GetMonsterData(int monsterId)
        {
            return monsterDatas.characterDatas[monsterId.ToString()];
        }
        
        List<WaveData> GetWaveData()
        {
            var waveId = PlayerPrefs.GetInt("MonsterWaveGroup");
            return ConfigDataManager.Instance.GetConfigData<WaveDataCollection>().waveDatas[waveId.ToString()];
        }
    }
    
    [Serializable]
    public class WaveData
    {
        public int waveId;
        public int waveGroup;
        public int nextTime;
        public float attack;
        public float maxHP;
        public List<List<int>> monsters;
    }
    
    [Serializable]
    public class WaveDataCollection
    {
        public Dictionary<string, List<WaveData>> waveDatas;
    }
}
