using System.Collections.Generic;
using Config;
using Evironment.MapGenerator;
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
        
        private MapDetail mapDetail;
        private Text text;
        private int mapDetailId;
        private int monsterWaveGroup;
        private bool isLimitedMap;
        
        void Awake()
        {
            text ??= GetComponentInChildren<Text>();
            lockIcon ??= gameObject.transform.Find("Lock").gameObject;
        }
        public void SetMapData(MapData mapData, bool isLimitedMap)
        {
            text.text = mapData.mapName;
            mapDetailId = mapData.mapDetailId;
            monsterWaveGroup = mapData.monsterWaveGroup;
            this.isLimitedMap = isLimitedMap;
            mapDetail = MapDetailDataCollection.mapDetailDataCollection.MapDetails[mapDetailId.ToString()];
        }
        
        public void LoadLevel()
        {
            PlayerPrefs.SetInt("IsLimitedMap", isLimitedMap? 1 : 0);
            PlayerPrefs.SetInt("ViewDistance", mapDetail.viewDistance);
            PlayerPrefs.SetInt("UnloadDistance", mapDetail.unloadDistance);
            PlayerPrefs.SetInt("TileSpacing", mapDetail.tileSpacing);
            PlayerPrefs.SetFloat("ObstacleSpawnRatio", mapDetail.obstacleSpawnRatio);
            PlayerPrefs.SetString("ObjectPoolAssetsPath", mapDetail.objectPoolAssetsPath);
            PlayerPrefs.SetInt("MonsterWaveGroup", monsterWaveGroup);
            Addressables.LoadSceneAsync(genericLevelSceneKey, LoadSceneMode.Single);
        }
    }
}