using Entity.Attack;
using UnityEngine;

namespace Entity.Player
{
    public class ArcherBehavior : PlayerController, IRangedAttack
    {
        [Header("Archer Settings")]
        public GameObject arrowPrefab;
        public float arrowSpeed = 15f;
        public float arrowLifeTime = 5f;
        public float shootingOffset = 0.3f;
        public float shootingAngle = 10f;

        public void ShootProjectile()
        {
            Vector3 shootingDirection = Quaternion.AngleAxis(-shootingAngle, transform.right) * transform.forward;
            
            GameObject arrow = Instantiate(
                arrowPrefab,
                transform.position + shootingOffset * Vector3.up,
                Quaternion.LookRotation(shootingDirection)
                );
            
            Rigidbody rb = arrow.GetComponent<Rigidbody>();
            rb.velocity = shootingDirection * arrowSpeed;

            // Set the arrow behavior
            ProjectileBehavior arrowBehavior = arrow.GetComponent<ProjectileBehavior>();
            arrowBehavior.lifeTime = arrowLifeTime;
            arrowBehavior.destroyOnCollision = false;
            arrowBehavior.rotateBasedOnVelocity = true;
        }
    }
}