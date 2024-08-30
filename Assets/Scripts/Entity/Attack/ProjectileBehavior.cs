using System;
using System.Collections;
using System.Collections.Generic;
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
        
        protected LayerMask obstacleLayer;
        protected Rigidbody rb;
        protected BulletMovement bulletMovement;

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
            bulletMovement = GetComponent<BulletMovement>();
            if (bulletMovement?.attackLogics != null)
            {
                foreach (var logic in bulletMovement.attackLogics)
                {
                    switch (logic.logic)
                    {
                        case "BulletRotate":
                            RotateLogic(logic.args);
                            break;
                    }
                }
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
                PlayDestroyFX();
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
        
        void PlayDestroyFX()
        {
            if (bulletMovement.config.destroyFX)
            {
                var destroyFX = Instantiate(bulletMovement.config.destroyFX, transform.position, Quaternion.identity);
                Destroy(destroyFX, 5f);
            }
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

        void RotateLogic(Dictionary<string, float> args)
        {
            var speed = args["speed"];
            StartCoroutine(RotateProjectile(speed));
        }
        
        IEnumerator RotateProjectile(float speed)
        {
            while (true)
            {
                transform.Rotate(Vector3.up, speed * 720 * Time.deltaTime);
                yield return null;
            }
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