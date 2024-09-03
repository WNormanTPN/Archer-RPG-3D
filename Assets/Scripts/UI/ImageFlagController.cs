using DG.Tweening;
using UnityEngine;

namespace UI
{
    public class ImageFlagController : MonoBehaviour
    {
        public AudioSource audioSource;
        
        public void MoveTo(Transform target)
        {
            audioSource.Play();
            transform.DOMoveX(target.position.x, 0.5f);
        }
    }
}
