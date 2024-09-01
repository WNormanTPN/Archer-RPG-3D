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
        [SerializeField]
        private bool _isCollideWithObstacle = true;
        public bool isCollideWithObstacle
        {
            get => _isCollideWithObstacle;
            set
            {
                _isCollideWithObstacle = value;
                if (!value)
                {
                    attachToCollidedObject = false;
                }
            }
        }
        [ShowWhen("_isCollideWithObstacle", true)] public bool attachToCollidedObject = false;
        public bool reboundWall = false;
        [ShowWhen("_throughEnemy", false)] 
        [SerializeField]
        private bool _bulletEject = false;
        public bool bulletEject
            {
            get => _bulletEject;
            set
            {
                _bulletEject = value;
                if (value)
                {
                    throughEnemy = false;
                }
            }
        }
        [ShowWhen("_bulletEject", false)]
        [SerializeField]
        private bool _throughEnemy = false;
        public bool throughEnemy
        {
            get => _throughEnemy;
            set
            {
                _throughEnemy = value;
                if (value)
                {
                    bulletEject = false;
                }
            }
        }
        
        
        protected LayerMask obstacleAndWallLayer;
        protected LayerMask enemyLayer;
        protected Rigidbody rb;
        protected BulletMovement bulletMovement;
        
        private RaycastHit hitPredicted;
        

        void Awake()
        {
            obstacleAndWallLayer = LayerMask.NameToLayer("Obstacle");
            if (gameObject.layer == LayerMask.NameToLayer("Player Immunity"))
            {
                enemyLayer = LayerMask.NameToLayer("Enemy");
            }
            else
            {
                enemyLayer = LayerMask.NameToLayer("Player");
            }
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
            reboundWall = bulletMovement is StraightMovement && (reboundWall || bulletMovement.config.wallRebound);
            bulletEject = bulletMovement is StraightMovement && (bulletEject || bulletMovement.config.eject);
            throughEnemy = throughEnemy || bulletMovement.config.penetration;
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
        
        void Update()
        {
            if (rb.isKinematic) return;

            int layermask = 0;

            if (reboundWall)
                layermask |= 1 << obstacleAndWallLayer;

            if (bulletEject)
                layermask |= 1 << enemyLayer;

            if (layermask != 0)
            {
                Physics.Raycast(transform.position,
                    rb.velocity.normalized,
                    out hitPredicted,
                    rb.velocity.magnitude * Time.deltaTime * 5,
                    layermask);
            }
        }

        
        void OnTriggerEnter(Collider collider)
        {
            if (collider.gameObject.layer == enemyLayer)
            {
                // Get the enemy's ICharacter component in collider or its parent
                var enemy = collider.GetComponentInParent<ICharacter>();
                enemy.TakeDamage(bulletMovement.config.damage);
                
                if (bulletEject)
                {
                    Reflect();
                }
                else if (isCollideWithObstacle)
                {
                    StopAllCoroutines();
                    PlayDestroyFX();
                    if (attachToCollidedObject)
                    {
                        AttachToCollidedObject(collider);
                    }
                    else
                    {
                        Destroy(gameObject);
                    }
                }
            }
            else if (reboundWall)
            {
                ReboundWall(collider);
            }
            else if (isCollideWithObstacle)
            {
                StopAllCoroutines();
                if (attachToCollidedObject)
                {
                    AttachToCollidedObject(collider);
                }
                else
                {
                    PlayDestroyFX();
                    Destroy(gameObject);
                }
            }
        }
        
        private void ReboundWall(Collider collider)
        {
            if (collider.gameObject.layer == obstacleAndWallLayer)
            {
                Reflect();
            }
        }
        
        private void BulletEject(Collider collider)
        {
            if (collider.gameObject.layer == enemyLayer)
            {
                Reflect();
            }
        }

        private void Reflect()
        {
            var straightMovement = bulletMovement as StraightMovement;
            if (hitPredicted.collider != null)
            {
                var normal = hitPredicted.normal;
                var reflect = Vector3.Reflect(rb.velocity, normal);
                straightMovement.direction = reflect.normalized;
            }
            else
            {
                straightMovement.direction = -straightMovement.direction;
            }
        }

        void PlayDestroyFX()
        {
            if (bulletMovement.config.destroyFX)
            {
                var destroyFX = Instantiate(bulletMovement.config.destroyFX, transform.position, Quaternion.identity);
                Destroy(destroyFX, 5f);
            }
        }
        
        void AttachToCollidedObject(Collider collider)
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
            transform.parent = collider.transform;
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