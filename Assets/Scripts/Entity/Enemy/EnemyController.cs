using System;
using System.Collections;
using Entity.Attack;
using UnityEngine;

namespace Entity.Enemy
{
    public abstract class EnemyController : MonoBehaviour, ICharacter, IHealth
    {
        [Header("Movement Settings")]
        public Transform player;                                // Reference to the player
        [Range(0, 10)] public float moveSpeed = 2f;             // Speed of the enemy movement
        [Range(0, 5)] public float moveDuration = 1.5f;         // Duration for moving forward before re-evaluating
        [Range(0, 5)] public float rotateDuration = 1f;         // Duration for rotating towards the player
        [Range(0, 100)] public float keepMovingDistance = 10f;  // Distance to keep moving towards the player

        [Header("Attack Settings")] 
        [SerializeField] private float _maxHealth = 20f;        // Health of the enemy
        public string attackAnimation;                          // Name of the attack animation
        [Range(0, 100)] public float rangeForAttack = 2f;       // Range at which the enemy starts attacking
        [Range(0, 10)] public float attackSpeed = 1f;           // Speed of the enemy attack per second
        public float attackCooldown = 1f;                       // Cooldown time between attacks

        protected bool isMovingForward;           // Flag to check if enemy is moving forward
        protected bool isRotating;                // Flag to check if enemy is rotating
        protected bool isAttacking;               // Flag to check if the enemy is attacking
        protected Rigidbody rb;                   // Reference to the Rigidbody component
        protected Animator animator;              // Reference to the Animator component

        private float _curHealth;                 // Current health of the enemy
        private float moveTimer;                  // Timer to track movement duration
        private float rotateTimer;                // Timer to track rotation duration
        private float attackTimer;                // Timer for attack cooldown
        private readonly string speedParameter = "Speed";
        private readonly string attackSpeedParameter = "AttackSpeed";
        
        
        public float curHealth { get => _curHealth; set => _curHealth = value; }
        public float maxHealth { get => _maxHealth; set => _maxHealth = value; }
        
        void Start()
        {
            moveTimer = moveDuration;
            isMovingForward = true;
            rb = GetComponent<Rigidbody>();
            animator = GetComponent<Animator>();
            attackTimer = attackCooldown;
            curHealth = maxHealth;
        }

        protected void FixedUpdate()
        {
            if (player)
            {
                float distanceToPlayer = Vector3.Distance(transform.position, player.position);

                if (distanceToPlayer <= rangeForAttack)
                {
                    // Within attack range, stop moving and attack
                    StopMove();
                    LockOnPlayer();
                    if (!isAttacking && attackTimer >= attackCooldown)
                    {
                        StartCoroutine(PerformAttack());
                    }
                }
                else if (distanceToPlayer >= keepMovingDistance)
                {
                    isMovingForward = true;
                    isRotating = true;
                            
                    Vector3 directionToPlayer = player.position - transform.position;
                    Move(directionToPlayer);
                    Rotate(directionToPlayer);
                }
                else
                {
                    // Handle movement and rotation
                    if (isMovingForward)
                    {
                        // Move forward towards the player
                        Move(transform.forward);
                        moveTimer -= Time.fixedDeltaTime;

                        if (moveTimer <= 0f)
                        {
                            // Stop moving forward and check player position
                            isMovingForward = false;
                            moveTimer = moveDuration; // Reset timer
                            isRotating = true; // Start rotating towards player
                        }
                    }
                    else if (isRotating)
                    {
                        StopMove();
                        
                        rotateTimer += Time.fixedDeltaTime;
                        Vector3 directionToPlayer = player.position - transform.position;
                        Rotate(directionToPlayer);
                        
                        if (rotateTimer >= rotateDuration)
                        {
                            // Stop rotating and start moving forward
                            isRotating = false;
                            isMovingForward = true;
                            rotateTimer = 0f; // Reset timer
                        }
                    }
                }

                // Update attack timer
                if (attackTimer < attackCooldown)
                {
                    attackTimer += Time.fixedDeltaTime;
                }
            }
            else
            {
                player = GameObject.FindGameObjectWithTag("Player").transform;
            }
        }

        public void Move(Vector3 direction) // Move forward towards the player
        {
            if (animator.GetCurrentAnimatorStateInfo(0).IsName(attackAnimation)) return;
            rb.position += moveSpeed * Time.fixedDeltaTime * transform.forward;
            animator.SetFloat(speedParameter, moveSpeed);
        }

        public void Rotate(Vector3 direction) // Rotate towards the player based on the duration
        {
            direction.y = 0; // Ignore y-axis
            direction = direction.normalized;
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotateTimer / rotateDuration);
        }
        
        void LockOnPlayer()
        {
            Vector3 directionToPlayer = player.position - transform.position;
            directionToPlayer.y = 0; // Ignore y-axis
            directionToPlayer.Normalize();
            Quaternion targetRotation = Quaternion.LookRotation(directionToPlayer);
            transform.rotation = targetRotation;
        }

        public void StopMove()
        {
            rb.velocity = Vector3.zero; // Stop any existing movement
            animator.SetFloat(speedParameter, 0f);
        }

        public void Attack()
        {
            isAttacking = true;
            animator.SetBool(attackAnimation, true);
        }
        
        public void StopAttack()
        {
            isAttacking = false;
            animator.SetBool(attackAnimation, false);
        }
        
        void SetAnimationAttackSpeed()
        {
            float animationLength = animator.GetCurrentAnimatorClipInfo(0)[0].clip.length;
            float animationMultiplier = attackSpeed * animationLength;
            animator.SetFloat(attackSpeedParameter, animationMultiplier);
        }

        private IEnumerator PerformAttack()
        {
            Attack();
            attackTimer = -1 / attackSpeed; // Reset attack timer

            yield return new WaitUntil(() => animator.GetCurrentAnimatorStateInfo(0).IsName(attackAnimation));
            SetAnimationAttackSpeed();
        }
    }
}
