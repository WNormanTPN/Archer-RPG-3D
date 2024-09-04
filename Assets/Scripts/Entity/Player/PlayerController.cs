using System.Collections;
using Config;
using MyEditor;
using UI;
using UnityEngine;
using UnityEngine.UIElements;
using Slider = UnityEngine.UI.Slider;

namespace Entity.Player
{
    public class PlayerController : CharacterBase
    {
        [InspectorGroup("Attack Settings")]
        public GameObject damageFX;                           // Reference to the damage effect

        [LastGroup] 
        public int level = 0;
        public int expPerLevel = 100;

        [InspectorGroup("Character Movement")] 
        public GameObject footDustPrefab;

        protected int curExp { get; private set; }
        protected PlayerLevelUpManager levelUpManager;
        
        private int playerMaxLevel = 0;
        private MyInput input;                                // Reference to the MyInput script
        private readonly string idleAnimation = "Idle";
        private readonly string attackAnimation = "Attack_bow";
        private AudioSource walkingSound;
        private GameObject footDust;
        private Slider healthBar;
        
        protected override void Start()
        {
            walkingSound = GetComponent<AudioSource>();
            levelUpManager = FindObjectOfType<PlayerLevelUpManager>();
            healthBar = GameObject.FindGameObjectWithTag("PlayerHpBar").GetComponentInChildren<Slider>();
            base.Start();
            SetUpCharacter(characterInitData);
            
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
                StopWalkingFX();
            }
            else
            {
                PlayWalkingFX();
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
            if (healthBar)
                healthBar.value = (float)curHealth / maxHealth;
            StartCoroutine(PlayDamageEffect());
        }

        protected override void LoadInitData()
        {
            base.LoadInitData();
            CalculateMaxLevel();
            AddExp(characterInitData.exp);
        }
        
        private void PlayWalkingFX()
        {
            if (!walkingSound.isPlaying)
                walkingSound.Play();

            if (!footDust)
            {
                footDust = Instantiate(footDustPrefab, transform.position, Quaternion.identity);
                Destroy(footDust, 0.5f);
            }
        }
        
        private void StopWalkingFX()
        {
            walkingSound.Stop();
        }
        
        private void CalculateMaxLevel()
        {
            var skillsConfig = Skill.skillCollection;
            foreach (var skill in skillsConfig.Skills)
            {
                playerMaxLevel += skill.Value.maxStacks;
            }
            playerMaxLevel -= characterInitData.skillIds.Count;
        }
        
        public void AddExp(int exp)
        {
            curExp += exp;
            while (curExp >= expPerLevel && level < playerMaxLevel)
            {
                LevelUp();
                curExp -= expPerLevel;
            }
            if (level >= playerMaxLevel)
            {
                curExp = expPerLevel;
            }
        }
        
        protected void LevelUp()
        {
            level++;
            levelUpManager.StartLevelUpProcess();
            
            var healthIncrease = (int) (maxHealth * 0.1f * level);
            maxHealth += healthIncrease;
            curHealth += healthIncrease;
        }

        private IEnumerator PlayDamageEffect()
        {
            damageFX.SetActive(true);
            yield return new WaitForSeconds(0.5f);
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