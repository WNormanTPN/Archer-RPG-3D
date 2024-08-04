using DG.Tweening;
using UnityEngine;

namespace UI
{
    public class ImageFlagController : MonoBehaviour
    {
        public void MoveTo(Transform target)
        {
            transform.DOMoveX(target.position.x, 0.5f);
        }
    }
}
