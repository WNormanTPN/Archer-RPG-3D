using System;
using System.Collections;
using Entity.Attack;
using UnityEngine;

namespace Entity.Enemy
{
    public class RangedAttackEnemy : EnemyController, IRangedAttack
    {
        [Header("Ranged Attack Settings")]
        [Range(0, 100)] public float attackDamage = 10f;    // Damage dealt by the enemy
        public GameObject lineRenderer;                     // Line renderer for the shooting effect
        public float lineRendererWidth = 0.5f;              // Width of the line renderer
        public GameObject projectilePrefab;                 // Projectile to shoot
        public float projectileSpeed = 10f;                 // Speed of the projectile
        public float projectileLifeTime = 5f;
        public float shootingOffset = 0.3f;
        
        private GameObject lineRendererInstance;

        protected override void FixedUpdate()
        {
            base.FixedUpdate();
            if (!lineRenderer) return;
            if (isAttacking)
            {
                // Draw trajectory line
                if (!lineRendererInstance)
                {
                    lineRendererInstance = Instantiate(lineRenderer);
                }

                var lineRendererComponent = lineRendererInstance.GetComponent<LineRenderer>();
                Vector3 shootingDirection = transform.forward;
                Vector3 shootingPosition = transform.position + shootingOffset * Vector3.up;
                DrawTrajectoryLine(lineRendererComponent, shootingPosition, shootingDirection);
            }
            else
            {
                if (lineRendererInstance)
                {
                    Destroy(lineRendererInstance);
                }
            }
        }
        
        void DrawTrajectoryLine(LineRenderer lineRenderer, Vector3 start, Vector3 direction)
        {
            Vector3 end = start + direction * 100f;
            Ray ray = new Ray(start, direction);
            RaycastHit[] hits = new RaycastHit[10];
            int size = Physics.RaycastNonAlloc(ray, hits, 100f, LayerMask.NameToLayer("Enemy"));
            
            foreach (var hit in hits)
            {
                if (hit.collider.gameObject.layer != LayerMask.NameToLayer("Enemy"))
                {
                    end = hit.point;
                    break;
                }
            }
            
            lineRenderer.startWidth = lineRendererWidth;
            lineRenderer.endWidth = lineRendererWidth;
            lineRenderer.SetPosition(0, start);
            lineRenderer.SetPosition(1, end);
        }

        public void ShootProjectile()
        {
            StopAttack();
            if (lineRendererInstance != null)
            {
                Destroy(lineRendererInstance);
            }
            
            // Instantiate projectile
            Vector3 shootingDirection = transform.forward;
            
            GameObject projectile = Instantiate(
                projectilePrefab,
                transform.position + shootingOffset * Vector3.up,
                Quaternion.identity
            );
            
            Rigidbody projectileRb = projectile.GetComponent<Rigidbody>();
            projectileRb.velocity = shootingDirection * projectileSpeed;

            // Get the ArrowBehavior component and start the rotation correction coroutine
            ProjectileBehavior projectileBehavior = projectile.GetComponent<ProjectileBehavior>();

            // Destroy the arrow after the specified lifetime
            Destroy(projectile, projectileLifeTime);
        }
    }
}