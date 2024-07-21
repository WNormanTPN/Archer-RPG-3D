using System.Collections;
using UnityEngine;

namespace Entity.Attack
{
    public class ArrowBehavior : MonoBehaviour
    {
        private Rigidbody rb;

        private void Awake()
        {
            rb = GetComponent<Rigidbody>();
        }

        private void OnCollisionEnter(Collision collision)
        {
            // Stop the arrow's movement upon collision
            if (!rb.isKinematic)
            {
                rb.velocity = Vector3.zero;
            }
            rb.isKinematic = true;

            // Optionally, disable the collider to prevent further collisions
            GetComponent<Collider>().enabled = false;

            // Stop the rotation coroutine
            StopAllCoroutines();
        }

        public void StartRotationCorrection()
        {
            StartCoroutine(RotateArrow());
        }

        private IEnumerator RotateArrow()
        {
            while (true)
            {
                Vector3 velocity = rb.velocity;
                if (velocity != Vector3.zero)
                {
                    transform.rotation = Quaternion.LookRotation(velocity);
                }
                yield return null;
            }
        }
    }
}