using System.Collections;
using UnityEngine;

namespace Entity.Enemy
{
    public abstract class EnemyController : MonoBehaviour, ICharacter
    {
        public Transform player;                // Reference to the player
        [Range(0, 10)] public float moveSpeed = 2f;            // Speed of the enemy movement
        [Range(0, 5)] public float moveDuration = 1.5f;         // Duration for moving forward before re-evaluating
        [Range(0, 5)] public float rotateDuration = 1f;         // Duration for rotating towards the player
        [Range(0, 100)]public float attackRange = 2f;          // Range at which the enemy starts attacking
        [Range(0, 10)] public float attackDelay = 1f;  // Delay between attacks

        private float moveTimer;                // Timer to track movement duration
        protected bool isMovingForward;           // Flag to check if enemy is moving forward
        private float rotateTimer;               // Timer to track rotation duration
        protected bool isRotating;                // Flag to check if enemy is rotating
        protected bool isAttacking;               // Flag to check if the enemy is attacking
        protected Rigidbody rb;                   // Reference to the Rigidbody component
        protected Animator animator;              // Reference to the Animator component

        void Start()
        {
            moveTimer = moveDuration;
            isMovingForward = true;
            rb = GetComponent<Rigidbody>();
            animator = GetComponent<Animator>();
        }

        void FixedUpdate()
        {
            if (player)
            {
                float distanceToPlayer = Vector3.Distance(transform.position, player.position);

                if (distanceToPlayer <= attackRange)
                {
                    // Within attack range, stop moving and attack
                    StopMove();
                    LockOnPlayer();
                    if (!isAttacking)
                    {
                        Attack();
                    }
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
                        Vector3 directionToPlayer = (player.position - transform.position).normalized;
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
            }
            else
            {
                player = GameObject.FindGameObjectWithTag("Player").transform;
            }
        }

        public void Move(Vector3 direction) // Move forward towards the player
        {
            rb.position += moveSpeed * Time.fixedDeltaTime * transform.forward;
            animator.SetFloat("Speed", moveSpeed);
        }

        public void Rotate(Vector3 direction) // Rotate towards the player base on the duration
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotateTimer / rotateDuration);
        }
        
        void LockOnPlayer()
        {
            Vector3 directionToPlayer = (player.position - transform.position).normalized;
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
            animator.SetBool("Attack_bow", true);
            animator.SetFloat("AttackSpeed", 1 / attackDelay);
        }
        
        public void StopAttack()
        {
            isAttacking = false;
            animator.SetBool("Attack_bow", false);
        }
    }
}
