using System.Collections.Generic;
using Config;
using Entity;
using Generic;
using Newtonsoft.Json;
using UnityEngine;

namespace UI
{
    public class MapLoader : MonoBehaviour
    {
        public GameObject sceneItemPrefab;      // Assign your SceneItem prefab in the Inspector
        public Transform defaultModeContent;    // Assign the content container for DefaultMode in the Inspector
        public Transform endlessModeContent;    // Assign the content container for EndlessMode in the Inspector
        public GameObject nullPrefab;           // Assign the null prefab in the Inspector

        private MapDataCollection mapCollection;

        void Start()
        {
            mapCollection = ConfigDataManager.Instance.GetConfigData<MapDataCollection>();
            CreateMapUI(mapCollection.DefaultMode, defaultModeContent);
            CreateMapUI(mapCollection.EndlessMode, endlessModeContent);
        }

        void CreateMapUI(List<MapData> mapMode, Transform contentContainer)
        {
            foreach (var map in mapMode)
            {
                GameObject sceneItem = Instantiate(sceneItemPrefab, contentContainer);
                SceneItem sceneItemComponent = sceneItem.GetComponent<SceneItem>();
                if (sceneItemComponent != null)
                {
                    var isLimitedMap = mapMode == mapCollection.EndlessMode;
                    sceneItemComponent.SetMapData(map, isLimitedMap);
                }
            }
            Instantiate(nullPrefab, contentContainer);
        }
    }

    [System.Serializable]
    public class MapData
    {
        public int mapId;
        public string mapName;
        public int mapDetailId;
        public int monsterWaveGroup;
    }

    [System.Serializable]
    public class MapDataCollection : IConfigCollection
    {
        public List<MapData> DefaultMode;
        public List<MapData> EndlessMode;
        
        public MapDataCollection() {}

        [JsonConstructor]
        public MapDataCollection(List<MapData> defaultMode, List<MapData> endlessMode)
        {
            DefaultMode = defaultMode;
            EndlessMode = endlessMode;
        }

        public void FromJson(string json)
        {
            var data = JsonConvert.DeserializeObject<MapDataCollection>(json);
            DefaultMode = data.DefaultMode;
            EndlessMode = data.EndlessMode;
        }
    }

}