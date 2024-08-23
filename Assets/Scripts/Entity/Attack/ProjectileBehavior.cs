using System;
using System.Collections;
using MyEditor;
using UnityEditor;
using UnityEngine;

namespace Entity.Attack
{
    public class ProjectileBehaviour : MonoBehaviour
    {
        public float lifeTime = 0f;
        public bool rotateBasedOnVelocity = true;
        public bool isCollideWithObstacle = true;
        [ShowWhen("isCollideWithObstacle", true)] public bool attachToCollidedObject = false;
        
        private LayerMask obstacleLayer;
        private Rigidbody rb;

        void Awake()
        {
            obstacleLayer = LayerMask.NameToLayer("Obstacle");
            rb = GetComponent<Rigidbody>();
            if (!isCollideWithObstacle)
            {
                attachToCollidedObject = false;
            }
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

        void OnCollisionEnter(Collision collision)
        {
            if (!isCollideWithObstacle && collision.gameObject.layer == obstacleLayer)
            {
                return;
            }
            
            if (attachToCollidedObject)
            {
                AttachToCollidedObject();
            }
            else
            {
                Destroy(gameObject);
            }

            // Stop the rotation coroutine
            StopAllCoroutines();
        }
        
        void OnTriggerEnter(Collider collider)
        {
            if (!isCollideWithObstacle && collider.gameObject.layer == obstacleLayer)
            {
                return;
            }
            
            if (attachToCollidedObject)
            {
                AttachToCollidedObject();
            }
            
            // Stop the rotation coroutine
            StopAllCoroutines();
        }
        
        void AttachToCollidedObject()
        {
            // Stop the projectile's movement upon collision
            if (!rb.isKinematic)
            {
                rb.velocity = Vector3.zero;
            }

            rb.isKinematic = true;

            // Optionally, disable the collider to prevent further collisions
            GetComponent<Collider>().enabled = false;

            // Parent the projectile to the collided object
            transform.parent = rb.transform;
        }
        
        void OnDestroy()
        {
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