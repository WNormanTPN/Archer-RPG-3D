using UnityEngine;

namespace Entity.Player
{
    public class PlayerController : MonoBehaviour
    {
        [Header("Movement Settings")]
        [Range(0, 10)] public float moveSpeed = 5f;           // Speed of the player movement
        [Range(0, 720)] public float rotationSpeed = 720f;    // Speed of the player rotation in degrees per second
        
        protected Animator animator;                          // Reference to the Animator component
        protected float velocity;                             // Current velocity of the player
        protected Rigidbody rb;                               // Reference to the Rigidbody component
        
        private MyInput input;                                // Reference to the MyInput script

        void Start()
        {
            input = new MyInput();
            input.Enable();
            rb = GetComponent<Rigidbody>();
            animator = GetComponent<Animator>();
        }

        void FixedUpdate()
        {
            // Handle input and set animation parameters
            Vector2 inputDirection = input.Player.Move.ReadValue<Vector2>();

            Vector3 movement = new Vector3(inputDirection.x, 0, inputDirection.y).normalized;
            if (movement == Vector3.zero)
            {
                StopPlayer();
                Attack();
            }
            else
            {
                StopAttack();
                MovePlayer(movement);
            }

            // Update animator parameters
            animator.SetFloat("Speed", movement.magnitude);
        }

        void Attack()
        {
            animator.SetBool("Attack_bow", true);
        }

        void StopAttack()
        {
            animator.SetBool("Attack_bow", false);
        }

        void MovePlayer(Vector3 direction)
        {
            // Move the player
            velocity = Mathf.Lerp(velocity, Time.fixedDeltaTime * moveSpeed * direction.magnitude, 0.1f);
            rb.position += velocity * direction;

            // Rotate player towards movement direction
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed * Time.fixedDeltaTime);
        }

        void StopPlayer()
        {
            velocity = 0f;
            // Update animator parameter to transition to idle
            animator.SetFloat("Speed", 0f);
        }
    
        void OnDisable()
        {
            input.Disable();
        }
    }
}