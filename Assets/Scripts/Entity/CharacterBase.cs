using System.Collections.Generic;
using Entity.Attack;
using UnityEngine;

namespace Entity
{
    public abstract class CharacterBase : MonoBehaviour, ICharacter
    {
        [Header("Character Settings")]
        [Range(0, 10)] public float moveSpeed = 5f;           // Speed of the character movement
        [Range(0, 720)] public float rotationSpeed = 720f;    // Speed of the character rotation in degrees per second
        [Range(0, 1000)]public int maxHealth = 100;           // Health of the character
        [Range(0, 1000)] public int curHealth = 100;          // Current health of the character
        
        [Header("Attack Settings")]
        [Range(0, 10)] public float attackSpeed = 1f;         // Speed of the character attack per second
        public Weapon weapon;                                 // Reference to the weapon
        public List<Skill> skills;                            // List of skills the character has
        public int exp;                                       // Experience points of the character
    
        
        protected Animator animator;                          // Reference to the Animator component
        protected float velocity;                             // Current velocity of the character
        protected Rigidbody rb;                               // Reference to the Rigidbody component
        
        
        protected virtual void Start()
        {
            rb = GetComponent<Rigidbody>();
            animator = GetComponent<Animator>();
        }
        
        
        public virtual void Move(Vector3 direction)
        {
            velocity = Mathf.Lerp(velocity, Time.fixedDeltaTime * moveSpeed * direction.magnitude, 0.1f);
            rb.position += velocity * direction;
        }

        public virtual void StopMove()
        {
            velocity = 0f;
        }
        
        public virtual void Rotate(Vector3 direction)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.RotateTowards(
                transform.rotation, targetRotation, rotationSpeed * Time.fixedDeltaTime);
        }
        public abstract void Attack();
        public abstract void StopAttack();

        public virtual void SetScale(float scale)
        {
            transform.localScale = new Vector3(scale, scale, scale);
        }
    }
}
