using System.Collections.Generic;
using Generic;
using UnityEngine;

namespace UI
{
    public class MapLoader : MonoBehaviour
    {
        public TextAsset mapJsonFile;           // Assign your JSON file in the Inspector
        public GameObject sceneItemPrefab;      // Assign your SceneItem prefab in the Inspector
        public Transform defaultModeContent;    // Assign the content container for DefaultMode in the Inspector
        public Transform endlessModeContent;    // Assign the content container for EndlessMode in the Inspector
        public GameObject nullPrefab;           // Assign the null prefab in the Inspector

        private MapCollection mapCollection;

        void Start()
        {
            mapCollection = JSONLoader.LoadJSON<MapCollection>(mapJsonFile);
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
                    sceneItemComponent.SetMapData(map);
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
        public string mapRes;
        public int monsterWaveGroup;
    }

    [System.Serializable]
    public class MapCollection
    {
        public List<MapData> DefaultMode;
        public List<MapData> EndlessMode;
    }
}