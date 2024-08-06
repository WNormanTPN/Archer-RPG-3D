using System.Collections;
using UnityEngine;

namespace Entity.Player
{
    public abstract class PlayerController : MonoBehaviour, ICharacter
    {
        [Header("Movement Settings")]
        [Range(0, 10)] public float moveSpeed = 5f;           // Speed of the player movement
        [Range(0, 720)] public float rotationSpeed = 720f;    // Speed of the player rotation in degrees per second
        [Range(0, 10)] public float attackSpeed = 1f;         // Speed of the player attack per second
        
        protected Animator animator;                          // Reference to the Animator component
        protected float velocity;                             // Current velocity of the player
        protected Rigidbody rb;                               // Reference to the Rigidbody component
        
        private MyInput input;                                // Reference to the MyInput script
        private readonly string speedParameter = "Speed";
        private readonly string idleAnimation = "Idle";
        private readonly string attackAnimation = "Attack_bow";
        private readonly string attackSpeedParameter = "AttackSpeed";

        protected virtual void Start()
        {
            input = new MyInput();
            input.Enable();
            rb = GetComponent<Rigidbody>();
            animator = GetComponent<Animator>();
        }

        protected virtual void FixedUpdate()
        {
            // Handle input and set animation parameters
            Vector2 inputDirection = input.Player.Move.ReadValue<Vector2>();

            Vector3 movement = new Vector3(inputDirection.x, 0, inputDirection.y).normalized;
            if (movement == Vector3.zero)
            {
                StopMove();
                Attack();
            }
            else
            {
                StopAttack();
                Move(movement);
                Rotate(movement);
            }

            // Update animator parameters
            animator.SetFloat(speedParameter, movement.magnitude);
        }

        public void Attack()
        {
            animator.SetBool(attackAnimation, true);
            StartCoroutine(SetAnimationAttackSpeed());
        }

        public void StopAttack()
        {
            animator.SetBool(attackAnimation, false);
            if(animator.GetCurrentAnimatorStateInfo(0).IsName(attackAnimation))
                animator.Play(idleAnimation);
        }

        public void Move(Vector3 direction)
        {
            // Move the player
            velocity = Mathf.Lerp(velocity, Time.fixedDeltaTime * moveSpeed * direction.magnitude, 0.1f);
            rb.position += velocity * direction;
        }

        public void Rotate(Vector3 direction)
        {
            // Rotate player towards movement direction
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed * Time.fixedDeltaTime);
        }

        public void StopMove()
        {
            velocity = 0f;
            // Update animator parameter to transition to idle
            animator.SetFloat(speedParameter, 0f);
        }
        
        IEnumerator SetAnimationAttackSpeed()
        {
            yield return new WaitUntil(() => animator.GetCurrentAnimatorStateInfo(0).IsName(attackAnimation));
            
            float animationLength = animator.GetCurrentAnimatorClipInfo(0).Length;
            float animationMultiplier = attackSpeed * animationLength;
            animator.SetFloat(attackSpeedParameter, animationMultiplier);
        }
    
        void OnDisable()
        {
            input.Disable();
        }
    }
}