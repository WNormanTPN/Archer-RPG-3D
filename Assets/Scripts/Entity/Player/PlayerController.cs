using System.Collections;
using MyEditor;
using UnityEngine;

namespace Entity.Player
{
    public class PlayerController : CharacterBase
    {
        [InspectorGroup("Attack Settings")]
        public GameObject damageFX;                           // Reference to the damage effect
        
        private MyInput input;                                // Reference to the MyInput script
        private readonly string idleAnimation = "Idle";
        private readonly string attackAnimation = "Attack_bow";
        
        protected override void Start()
        {
            base.Start();
            if (input == null)
            {
                input = new MyInput();
            }
            input.Enable();
        }

        protected virtual void FixedUpdate()
        {
            // Handle input and set animation parameters
            Vector2 inputDirection = input.Player.Move.ReadValue<Vector2>();

            Vector3 movement = new Vector3(inputDirection.x, 0, inputDirection.y).normalized;
            if (movement == Vector3.zero)
            {
                StopMove();
                StartAttackAnim();
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

        public override void StartAttackAnim()
        {
            animator.SetBool(attackAnimation, true);
            StartCoroutine(SetAnimationAttackSpeed());
        }

        public override void StopAttack()
        {
            animator.SetBool(attackAnimation, false);
            if(animator.GetCurrentAnimatorStateInfo(0).IsName(attackAnimation))
                animator.Play(idleAnimation);
        }

        public override void StopMove()
        {
            velocity = 0f;
            // Update animator parameter to transition to idle
            animator.SetFloat(speedParameter, 0f);
        }
        
        public override void TakeDamage(float damage)
        {
            base.TakeDamage(damage);
            StartCoroutine(PlayDamageEffect());
        }
        
        private IEnumerator PlayDamageEffect()
        {
            damageFX.SetActive(true);
            yield return new WaitForSeconds(0.1f);
            damageFX.SetActive(false);
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