using System;
using System.Collections;
using UnityEditor;
using UnityEngine;

namespace Entity.Attack
{
    public class ProjectileBehaviour : MonoBehaviour
    {
        public float lifeTime = 0f;
        public bool destroyOnCollision = true;
        public bool rotateBasedOnVelocity = true;
        
        private Rigidbody rb;
        private bool isCollided = false;

        void Awake()
        {
            rb = GetComponent<Rigidbody>();
        }
        
        void Start()
        {
            if (rb.velocity != Vector3.zero)
            {
                transform.rotation = Quaternion.LookRotation(rb.velocity);
            }
            if (rotateBasedOnVelocity)
            {
                StartRotationCorrection();
            }
            if(lifeTime > 0)
            {
                Destroy(gameObject, lifeTime);
            }
        }

        void OnDestroy()
        {
            if (isCollided && !destroyOnCollision)
            {
                return;
            }
        }

        void OnCollisionEnter(Collision collision)
        {
            if (destroyOnCollision)
            {
                Destroy(gameObject);
                return;
            }
            
            isCollided = true;
            // Stop the projectile's movement upon collision
            if (!rb.isKinematic)
            {
                rb.velocity = Vector3.zero;
            }
            rb.isKinematic = true;

            // Optionally, disable the collider to prevent further collisions
            GetComponent<Collider>().enabled = false;
            
            // Parent the projectile to the collided object
            transform.parent = collision.transform;

            // Stop the rotation coroutine
            StopAllCoroutines();
        }

        void StartRotationCorrection() {
            StartCoroutine(RotateProjectileBaseOnVelocity());
        }

        private IEnumerator RotateProjectileBaseOnVelocity()
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