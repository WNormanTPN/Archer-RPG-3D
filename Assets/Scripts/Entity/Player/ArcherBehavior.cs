using Entity.Attack;
using UnityEngine;

namespace Entity.Player
{
    public class ArcherBehavior : PlayerController, IRangedAttack, IHealth
    {
        [SerializeField] private float _maxHealth = 100f;
        [Header("Archer Settings")]
        public float attackDamage = 10f;
        public GameObject arrowPrefab;
        [Range(0, 100)] public float arrowSpeed = 15f;
        [Range(0, 10)] public float arrowLifeTime = 5f;
        public float shootingOffset = 0.3f;
        [Range(0, 90)] public float shootingAngle = 10f;

        private float _curHealth;
        
        public float curHealth { get => _curHealth; set => _curHealth = value; }
        public float maxHealth { get => _maxHealth; set => _maxHealth = value; }
        
        protected override void Start()
        {
            base.Start();
            curHealth = maxHealth;
        }

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