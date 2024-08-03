using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace UI
{
    public class SmoothScroll : MonoBehaviour, IEndDragHandler, IBeginDragHandler
    {
        public ScrollRect scrollRect;        // Reference to the ScrollRect component
        public float snapSpeed = 10f;        // Speed of snapping

        private RectTransform content;       // Reference to the content RectTransform
        private Vector2[] itemsPos;          // Array of items in the content
        private Vector2 targetPosition;      // Target position for snapping
        private bool isSnapping = false;     // Flag to check if currently snapping

        void Start()
        {
            content = scrollRect.content;
            var items = new RectTransform[content.childCount];
            itemsPos = new Vector2[items.Length];
            for (int i = 0; i < content.childCount; i++)
            {
                items[i] = content.GetChild(i) as RectTransform;
                itemsPos[i] = -items[i].anchoredPosition;
                itemsPos[i].y = 0; // Only snap horizontally
            }
        }

        void Update()
        {
            if (isSnapping)
            {
                SnapToNearestItem();
            }
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            // Begin snapping when dragging ends
            isSnapping = true;
            targetPosition = GetNearestItemPosition();
        }
        
        public void OnBeginDrag(PointerEventData eventData)
        {
            isSnapping = false;
        }

        private void SnapToNearestItem()
        {
            float distance = Vector2.Distance(content.anchoredPosition, targetPosition);

            // Smoothly move towards the target position
            content.anchoredPosition = Vector2.Lerp(content.anchoredPosition, targetPosition, Time.deltaTime * snapSpeed);

            // Check if we've reached the target position
            if (distance < 1f)
            {
                isSnapping = false;
                content.anchoredPosition = targetPosition; // Set exact position
            }
        }

        private Vector2 GetNearestItemPosition()
        {
            Vector2 curContentPosition = content.anchoredPosition;
            Vector2 nearestPosition = curContentPosition;
            float minDistance = float.MaxValue;

            foreach (Vector2 itemPos in itemsPos)
            {
                // Calculate the distance from the current position to the item
                float distance = Vector2.Distance(curContentPosition, itemPos);

                if (distance < minDistance)
                {
                    minDistance = distance;
                    nearestPosition = itemPos;
                }
            }
            
            return nearestPosition;
        }

        // New method to set the target index from outside
        public void SetTargetIndex(int index)
        {
            if (index >= 0 && index < itemsPos.Length)
            {
                targetPosition = itemsPos[index];
                isSnapping = true;
            }
            else
            {
                Debug.LogError("Index out of range");
            }
        }
    }
}
