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
            mapCollection = MapDataCollection.mapDataCollection;
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
                    var isLimitedMap = mapMode == mapCollection.DefaultMode;
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
        public static MapDataCollection mapDataCollection
        {
            get
            {
                if (_mapDataCollection == null)
                {
                    _mapDataCollection = new MapDataCollection(ConfigDataManager.Instance.GetConfigData<MapDataCollection>());
                }

                return _mapDataCollection;
            }
            private set => _mapDataCollection = value;
        }
        private static MapDataCollection _mapDataCollection;
        
        public MapDataCollection() {}

        [JsonConstructor]
        public MapDataCollection(List<MapData> defaultMode, List<MapData> endlessMode)
        {
            DefaultMode = defaultMode;
            EndlessMode = endlessMode;
        }

        public MapDataCollection(MapDataCollection data)
        {
            var copied = data.DeepCopy();
            DefaultMode = copied.DefaultMode;
            EndlessMode = copied.EndlessMode;
        }

        public void FromJson(string json)
        {
            var data = JsonConvert.DeserializeObject<MapDataCollection>(json);
            DefaultMode = data.DefaultMode;
            EndlessMode = data.EndlessMode;
        }
        
        public MapDataCollection DeepCopy()
        {
            string serializedObject = JsonConvert.SerializeObject(this);
            return JsonConvert.DeserializeObject<MapDataCollection>(serializedObject);
        }
    }

}