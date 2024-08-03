using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class SceneItem : MonoBehaviour
    {
        public GameObject lockIcon;
        private Text text;
        
        void Awake()
        {
            text ??= GetComponentInChildren<Text>();
            lockIcon ??= gameObject.transform.Find("Lock").gameObject;
        }
        public void SetMapData(MapData mapData)
        {
            text.text = mapData.mapName;
        }
    }
}