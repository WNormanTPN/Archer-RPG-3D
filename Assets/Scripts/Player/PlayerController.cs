using Unity.Mathematics;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public MyInput input;           // Reference to the MyInput script
    [Range(0, 10)] public float moveSpeed = 5f;   // Speed of the player movement
    [Range(0, 360)] public float rotationSpeed = 360f;  // Speed of the player rotation in degrees per second
    public Animator animator;      // Reference to the Animator component
    private float velocity = 0f;    // Current velocity of the player

    void Start()
    {
        input = new MyInput();
        input.Enable();
        if (animator == null)
        {
            // animator = GetComponent<Animator>();
        }
    }

    void Update()
    {
        // Handle input and set animation parameters
        Vector2 inputDirection = input.Player.Move.ReadValue<Vector2>();

        Vector3 movement = new Vector3(inputDirection.x, 0, inputDirection.y).normalized;
        if (movement == Vector3.zero)
        {
            StopPlayer();
        }
        else
        {
            MovePlayer(movement);
        }

        // Update animator parameters
        // animator.SetFloat("Speed", movement.magnitude);
    }

    void MovePlayer(Vector3 direction)
    {
        // Move the player
        velocity = Mathf.Lerp(velocity, Time.deltaTime * moveSpeed * direction.magnitude, 0.1f);
        transform.position += velocity * direction;

        // Rotate player towards movement direction
        Quaternion targetRotation = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
    }

    void StopPlayer()
    {
        velocity = 0f;
        // Update animator parameter to transition to idle
        // animator.SetFloat("Speed", 0f);
    }
    
    void OnDisable()
    {
        input.Disable();
    }
}