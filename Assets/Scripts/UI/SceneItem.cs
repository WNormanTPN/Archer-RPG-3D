using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace UI
{
    public class SceneItem : MonoBehaviour
    {
        public GameObject lockIcon;
        public string scenePath = "Scenes/";
        
        private Text text;
        private string mapResource;
        private int monsterWaveGroup;
        
        void Awake()
        {
            text ??= GetComponentInChildren<Text>();
            lockIcon ??= gameObject.transform.Find("Lock").gameObject;
        }
        public void SetMapData(MapData mapData)
        {
            text.text = mapData.mapName;
            mapResource = mapData.mapRes;
            monsterWaveGroup = mapData.monsterWaveGroup;
        }
        
        public void LoadLevel()
        {
            PlayerPrefs.SetInt("MonsterWaveGroup", monsterWaveGroup);
            SceneManager.LoadScene(scenePath + mapResource);
        }
    }
}