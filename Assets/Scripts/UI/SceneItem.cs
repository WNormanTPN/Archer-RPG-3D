using System.Collections.Generic;
using Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace UI
{
    public class SceneItem : MonoBehaviour
    {
        public GameObject lockIcon;
        public string genericLevelSceneKey;
        public TextAsset mapDetailConfig; 
        
        private Text text;
        private int mapDetailId;
        private int monsterWaveGroup;
        
        void Awake()
        {
            text ??= GetComponentInChildren<Text>();
            lockIcon ??= gameObject.transform.Find("Lock").gameObject;
        }
        public void SetMapData(MapData mapData)
        {
            text.text = mapData.mapName;
            mapDetailId = mapData.mapDetailId;
            monsterWaveGroup = mapData.monsterWaveGroup;
        }
        
        public void LoadLevel()
        {
            MapDetail mapDetail = JSONLoader.LoadJSON<Dictionary<string, MapDetail>>(mapDetailConfig)[mapDetailId.ToString()];
            PlayerPrefs.SetInt("ViewDistance", mapDetail.viewDistance);
            PlayerPrefs.SetInt("UnloadDistance", mapDetail.unloadDistance);
            PlayerPrefs.SetInt("TileSpacing", mapDetail.tileSpacing);
            PlayerPrefs.SetFloat("ObstacleSpawnRatio", mapDetail.obstacleSpawnRatio);
            PlayerPrefs.SetString("ObjectPoolAssetsPath", mapDetail.objectPoolAssetsPath);
            PlayerPrefs.SetInt("MonsterWaveGroup", monsterWaveGroup);
            Addressables.LoadSceneAsync(genericLevelSceneKey, LoadSceneMode.Single);
        }
    }
    
    public class MapDetail
    {
        public int mapDetailId;
        public int viewDistance;
        public int unloadDistance;
        public int tileSpacing;
        public float obstacleSpawnRatio;
        public string objectPoolAssetsPath;
    }
}