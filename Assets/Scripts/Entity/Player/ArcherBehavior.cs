using Entity.Attack;
using MyEditor;
using UnityEngine;

namespace Entity.Player
{
    public class ArcherBehavior : PlayerController, IRangedAttack
    {
        [InspectorGroup("Attack Settings")]
        public float attackDamage = 10f;
        public GameObject arrowPrefab;
        [Range(0, 100)] public float arrowSpeed = 15f;
        [Range(0, 10)] public float arrowLifeTime = 5f;
        public float shootingOffset = 0.3f;
        [Range(0, 90)] public float shootingAngle = 10f;
        
        protected override void Start()
        {
            base.Start();
            curHealth = maxHealth;
        }

        public void ShootProjectile()
        {
            weapon.DoAttack();
            // Vector3 shootingDirection = Quaternion.AngleAxis(-shootingAngle, transform.right) * transform.forward;
            //
            // GameObject arrow = Instantiate(
            //     arrowPrefab,
            //     transform.position + shootingOffset * Vector3.up,
            //     Quaternion.LookRotation(shootingDirection)
            //     );
            //
            // Rigidbody arrowRb = arrow.GetComponent<Rigidbody>();
            // arrowRb.velocity = shootingDirection * arrowSpeed;
            //
            // // Set the arrow behavior
            // ProjectileBehavior arrowBehavior = arrow.GetComponent<ProjectileBehavior>();
            // arrowBehavior.lifeTime = arrowLifeTime;
            // arrowBehavior.destroyOnCollision = false;
            // arrowBehavior.rotateBasedOnVelocity = true;
        }
    }
}