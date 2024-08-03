using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class LevelScrollController : MonoBehaviour
    {
        public ScrollRect scrollRect;
        public RectTransform content;
        public float centerScale = 1.2f;  // Scale factor for the centered item
        public float sideAlpha = 0.5f;    // Alpha for side items
        public float centerAlpha = 1.0f;  // Alpha for the centered item

        private int centerIndex;
        private List<RectTransform> items;
        private List<CanvasGroup> canvasGroups;
        private List<Button> buttons;
        private Vector2 defaultItemSize;
        

        void Start()
        {
            items = new List<RectTransform>();
            canvasGroups = new List<CanvasGroup>();
            buttons = new List<Button>();
        }

        void Update()
        {
            if (items.Count == 0)
            {
                foreach (Transform child in content)
                {
                    var canvasGroup = child.GetComponent<CanvasGroup>();
                    if (canvasGroup)
                    {
                        var rectTransform = child.GetComponent<RectTransform>();
                        var button = child.GetComponent<Button>();
                        items.Add(child.GetComponent<RectTransform>());
                        canvasGroups.Add(canvasGroup);
                        defaultItemSize = new Vector2(rectTransform.sizeDelta.x, rectTransform.sizeDelta.y);
                        buttons.Add(button);
                        button.enabled = false;
                    }
                }
            }
            UpdateItems();
        }

        void UpdateItems()
        {
            var scrollRectPosY = scrollRect.viewport.position.y;
            var scrollRecHalfHeight = scrollRect.viewport.rect.height / 2;
            var maxY = scrollRectPosY + scrollRecHalfHeight;
            var minY = scrollRectPosY - scrollRecHalfHeight;
            float minDistance = float.MaxValue;
            for (int i = 0; i < items.Count; i++)
            {
                var item = items[i];
                var canvasGroup = canvasGroups[i];
                var itemPosY = item.position.y;
                
                if (itemPosY > maxY || itemPosY < minY)
                {
                    canvasGroup.alpha = 0;
                }
                else
                {
                    if (buttons[centerIndex] && buttons[centerIndex].enabled) buttons[centerIndex].enabled = false;
                    var tValue = Mathf.Abs(itemPosY - scrollRectPosY) / scrollRecHalfHeight;
                    canvasGroup.alpha = Mathf.Lerp(sideAlpha, centerAlpha, 1 - tValue);
                    
                    var distance = Math.Abs(itemPosY - scrollRectPosY);
                    if (distance < minDistance)
                    {
                        minDistance = distance;
                        centerIndex = i;
                    }
                }
                item.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, defaultItemSize.x);
                item.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, defaultItemSize.y);
            }

            if (canvasGroups[centerIndex])
            {
                buttons[centerIndex].enabled = true;
                canvasGroups[centerIndex].alpha = centerAlpha;
                items[centerIndex]
                    .SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, defaultItemSize.x * this.centerScale);
                items[centerIndex]
                    .SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, defaultItemSize.y * this.centerScale);
            }
        }
    }
}
