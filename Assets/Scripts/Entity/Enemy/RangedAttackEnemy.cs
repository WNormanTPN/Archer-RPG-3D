using System.Collections;
using Entity.Attack;
using UnityEngine;

namespace Entity.Enemy
{
    public class RangedAttackEnemy : EnemyController, IRangedAttack
    {
        public GameObject projectilePrefab;  // Projectile to shoot
        public float projectileSpeed = 10f;  // Speed of the projectile
        public float projectileLifeTime = 5f;
        public float shootingOffset = 0.3f;

        public void ShootProjectile()
        {
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