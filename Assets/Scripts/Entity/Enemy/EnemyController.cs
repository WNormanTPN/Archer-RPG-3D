using System;
using System.Collections;
using UnityEngine;

namespace Entity.Enemy
{
    public abstract class EnemyController : MonoBehaviour, ICharacter
    {
        [Header("Movement Settings")]
        public Transform player;                                // Reference to the player
        [Range(0, 10)] public float moveSpeed = 2f;             // Speed of the enemy movement
        [Range(0, 5)] public float moveDuration = 1.5f;         // Duration for moving forward before re-evaluating
        [Range(0, 5)] public float rotateDuration = 1f;         // Duration for rotating towards the player
        [Range(0, 100)] public float keepMovingDistance = 10f;   // Distance to keep moving towards the player
        [Header("Attack Settings")]
        public string attackAnimation;                          // Name of the attack animation
        [Range(0, 100)] public float rangeForAttack = 2f;       // Range at which the enemy starts attacking
        [Range(0, 10)] public float attackSpeed = 1f;           // Speed of the enemy attack per second
        public float attackCooldown = 1f;                       // Cooldown time between attacks

        private float moveTimer;                  // Timer to track movement duration
        protected bool isMovingForward;           // Flag to check if enemy is moving forward
        private float rotateTimer;                // Timer to track rotation duration
        protected bool isRotating;                // Flag to check if enemy is rotating
        protected bool isAttacking;               // Flag to check if the enemy is attacking
        private float attackTimer;                // Timer for attack cooldown
        protected Rigidbody rb;                   // Reference to the Rigidbody component
        protected Animator animator;              // Reference to the Animator component

        void Start()
        {
            moveTimer = moveDuration;
            isMovingForward = true;
            rb = GetComponent<Rigidbody>();
            animator = GetComponent<Animator>();
            attackTimer = attackCooldown; // Initialize attack timer
        }

        void FixedUpdate()
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
                    if (isAttacking)
                    {
                        StopAttack();
                    }
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
            animator.SetFloat("Speed", moveSpeed);
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
            animator.SetFloat("Speed", 0f);
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
            animator.SetFloat("AttackSpeed", animationMultiplier);
        }

        private IEnumerator PerformAttack()
        {
            Attack();
            attackTimer = -1 / attackSpeed; // Reset attack timer

            yield return new WaitUntil(() => animator.GetCurrentAnimatorStateInfo(0).IsName(attackAnimation));
            SetAnimationAttackSpeed();

            StopAttack();
        }
    }
}
