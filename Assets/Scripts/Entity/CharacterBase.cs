using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Config;
using MyEditor;
using Newtonsoft.Json;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

namespace Entity
{
    [Serializable]
    public class CharacterDataCollection : IConfigCollection
    {
        public Dictionary<string, CharacterData> CharacterDatas;

        public CharacterDataCollection()
        {
            CharacterDatas = new Dictionary<string, CharacterData>();
        }
        
        [JsonConstructor]
        public CharacterDataCollection(Dictionary<string, CharacterData> CharacterDatas)
        {
            this.CharacterDatas = CharacterDatas;
        }

        public void FromJson(string json)
        {
            CharacterDatas = JsonConvert.DeserializeObject<Dictionary<string, CharacterData>>(json);
        }
    }
    
    public abstract class CharacterBase : MonoBehaviour, ICharacter
    {
        [InspectorGroup("Character Movement")]
        [Range(0, 10)] 
        [SerializeField]
        private float _moveSpeed = 5f;                        // Speed of the character movement
        public float moveSpeed
        {
            get => (_moveSpeed + moveSpeed_add) * moveSpeed_mul;
            set => _moveSpeed = value;
        }
        [Range(0, 720)]public float rotationSpeed = 720f;     // Speed of the character rotation in degrees per second
        
        [FirstGroup]
        [SerializeField] private Weapon _weapon;
        public Weapon weapon
        {
            get => _weapon;
            set
            {
                _weapon = value;
                _weapon.owner = gameObject;
            }
        }                             // Reference to the weapon
        public List<Effect> effects;
        [HideInInspector] public SkillCollection skills;                        // List of skills the character has
        
        [LastGroup]
        public CharacterData characterInitData;               // Initial data of the character
    
        [InspectorGroup("Attack Settings")]
        [Range(0, 1000)]
        [SerializeField]
        private int _maxHealth = 100;                         // Health of the character
        public int maxHealth
        {
            get => (maxHealth_add + _maxHealth) * maxHealth_mul;
            set => _maxHealth = value;
        }
        [Range(0, 1000)]
        [SerializeField]
        private int _curHealth = 100;                         // Current health of the character
        public int curHealth
        {
            get => Mathf.Min((curHealth_add + _curHealth) * curHealth_mul, maxHealth);
            set => _curHealth = value;
        }
        [Range(0, 10)]
        [SerializeField]
        private float _attackSpeed = 1f;                      // Speed of the character attack per second
        public float attackSpeed
        {
            get => (attackSpeed_add + _attackSpeed) * attackSpeed_mul;
            set => _attackSpeed = value;
        }
        [Range(0, 1000)] public float attackDamage = 10f;     // Damage dealt by the character
        
        [FormerlySerializedAs("attackPoint")] 
        public Transform forwardAttackPoint;                  // Point where the attack will be executed

        #region Properties
        
        protected Animator animator;                          // Reference to the Animator component
        protected float velocity;                             // Current velocity of the character
        protected Rigidbody rb;                               // Reference to the Rigidbody component
        protected AttackConfig attackConfig;                  // Configuration of the attack
        protected Transform backwardAttackPoint;              // Point where the backward attack will be executed
        protected Transform leftsideAttackPoint;              // Point where the left attack will be executed
        protected Transform rightsideAttackPoint;             // Point where the right attack will be executed
        
        protected float moveSpeed_add = 0;                    // Added move speed from skills and effects
        protected float moveSpeed_mul = 1;                    // Multiplied move speed from skills and effects
        protected int curHealth_add = 0;                    // Added health from skills and effects
        protected int curHealth_mul = 1;                    // Multiplied health from skills and effects
        protected int maxHealth_add = 0;                    // Added max health from skills and effects
        protected int maxHealth_mul = 1;                    // Multiplied max health from skills and effects
        protected float attackSpeed_add = 0;                 // Added attack speed from skills and effects
        protected float attackSpeed_mul = 1;                 // Multiplied attack speed from skills and effects
        protected float attackDamage_add = 0;                // Added attack damage from skills and effects
        protected float attackDamage_mul = 1;                // Multiplied attack damage from skills and effects
        
        protected readonly string speedParameter = "Speed";
        protected readonly string attackSpeedParameter = "AttackSpeed";
        protected readonly string takeDamageParameter = "Damage";
        protected readonly string dieParameter = "Death";
        
        #endregion
        

        protected virtual void Awake()
        {
            
        }

        protected virtual void Start()
        {
            Debug.Log(gameObject.name + " started");
            rb = GetComponent<Rigidbody>();
            animator = GetComponent<Animator>();
            attackConfig = new AttackConfig();
            skills = new SkillCollection();
            effects = new List<Effect>();
            curHealth = maxHealth;
            attackConfig.damage = attackDamage;
            if (forwardAttackPoint)
            {
                CalculateAttackPoints();
                attackConfig.forwardAttackPoint = forwardAttackPoint;
                attackConfig.backwardAttackPoint = backwardAttackPoint;
                attackConfig.leftsideAttackPoint = leftsideAttackPoint;
                attackConfig.rightsideAttackPoint = rightsideAttackPoint;
            }
        }
        
        public virtual void SetUpCharacter(CharacterData data)
        {
            characterInitData = data;
            LoadInitData();
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

        public abstract void StartAttackAnim();

        public virtual void TriggerStartAttack()
        {
            weapon.TriggerStartAttack(attackConfig);
        }
        public virtual void TriggerDoAttack()
        {
            weapon.TriggerDoAttack(attackConfig);
        }
        public virtual void TriggerEndAttack()
        {
            weapon.TriggerEndAttack(attackConfig);
        }
        public abstract void StopAttack();

        public virtual void SetScale(float scale)
        {
            transform.localScale = new Vector3(scale, scale, scale);
        }
        
        public virtual void TakeDamage(float damage)
        {
            curHealth -= Mathf.RoundToInt(damage);
            if (curHealth <= 0)
            {
                Die();
            }
            else
            {
                animator.SetTrigger(takeDamageParameter);
            }
        }
        
        public virtual void Die()
        {
            animator.SetTrigger(dieParameter);
            foreach (var col in GetComponentsInChildren<Collider>())
            {
                col.enabled = false;
            }
            Destroy(gameObject, 1.5f);
        }

        private void CalculateAttackPoints()
        {
            var direction = forwardAttackPoint.position - transform.position;
            backwardAttackPoint = GameObject.Instantiate(forwardAttackPoint, transform);
            leftsideAttackPoint = GameObject.Instantiate(forwardAttackPoint, transform);
            rightsideAttackPoint = GameObject.Instantiate(forwardAttackPoint, transform);
            direction = Quaternion.Euler(0, 180, 0) * direction;
            backwardAttackPoint.position = transform.position + direction;
            backwardAttackPoint.rotation = forwardAttackPoint.rotation * Quaternion.Euler(0, 180, 0);
            direction = Quaternion.Euler(0, 90, 0) * direction;
            leftsideAttackPoint.position = transform.position + direction;
            leftsideAttackPoint.rotation = forwardAttackPoint.rotation * Quaternion.Euler(0, -90, 0);
            direction = Quaternion.Euler(0, -180, 0) * direction;
            rightsideAttackPoint.position = transform.position + direction;
            rightsideAttackPoint.rotation = forwardAttackPoint.rotation * Quaternion.Euler(0, 90, 0);
        }
        
        protected virtual void LoadInitData()
        {
            if (characterInitData == null)
            {
                Debug.LogError("characterInitData chưa được khởi tạo!");
                return;
            }
    
            SetScale(characterInitData.scale);

            if (characterInitData.weaponId != 0 && weapon?.weaponID != characterInitData.weaponId)
            {
                weapon = new Weapon(characterInitData.weaponId);
            }

            if (characterInitData.skillIds != null && characterInitData.skillIds.Count > 0)
            {
                foreach (var skillID in characterInitData.skillIds)
                {
                    AddSkill(skillID);
                }
            }
        }
        
        public virtual void AddSkill(int skillID)
        {
            var skill = new Skill(skillID);
            AddSkill(skill);
            foreach (var exclusion in skill.exclusions)
            {
                var exclusionSkill = Skill.skillCollection.Skills[exclusion.ToString()];
                AddOrRemoveSkillAttributes(exclusionSkill, true);
            }
        }
        
        public virtual void AddSkill(Skill skill)
        {
            skill = skills.AddSkill(skill);
            AddOrRemoveSkillAttributes(skill, false);
            if (skill.effectIDs == null) return;
            foreach (var effectID in skill.effectIDs)
            {
                AddEffect(effectID);
            }
        }
        
        public virtual void AddEffect(int effectID)
        {
            var effect = new Effect(effectID);
            AddEffect(effect);
        }
        
        public virtual void AddEffect(Effect effect)
        {
            effects.Add(effect);

            foreach (var kvp in effect.effectValues)
            {
                var key = kvp.Key;
                var value = kvp.Value;
                StartCoroutine(ApplyEffectValueCoroutine(key, value, effect.duration));
            }
    
            if (effect.duration > 0)
            {
                StartCoroutine(RemoveEffectAfterDuration(effect));
            }
        }

        private IEnumerator RemoveEffectAfterDuration(Effect effect)
        {
            yield return new WaitForSeconds(effect.duration);
            if (this && gameObject.activeInHierarchy)
            {
                effects.Remove(effect);
            }
        }


        private IEnumerator ApplyEffectValueCoroutine(string key, float value, int duration)
        {
            switch (key)
            {
                case "moveSpeed_add":
                    moveSpeed_add += value;
                    if (duration != -1)
                    {
                        yield return new WaitForSeconds(duration);
                        moveSpeed_add -= value;
                    }
                    break;
                
                case "moveSpeed_mul":
                    moveSpeed_mul += value;
                    if (duration != -1)
                    {
                        yield return new WaitForSeconds(duration);
                        moveSpeed_mul -= value;
                    }
                    break;
                
                case "curHealth_add":
                    curHealth_add += (int) value;
                    curHealth = Mathf.Min(curHealth, maxHealth);
                    if (duration != -1)
                    {
                        yield return new WaitForSeconds(duration);
                        curHealth_add -= (int) value;
                        curHealth = Mathf.Min(curHealth, maxHealth);
                    }
                    break;
                
                case "maxHealth_add":
                    maxHealth_add += (int) value;
                    curHealth += (int) value;
                    if (duration != -1)
                    {
                        yield return new WaitForSeconds(duration);
                        curHealth -= (int) value;
                        maxHealth_add -= (int) value;
                        curHealth = Mathf.Min(curHealth, maxHealth);
                    }
                    break;
                
                case "maxHealth_mul":
                    int oldMaxHealth = maxHealth;
                    maxHealth_mul += (int) value;
                    int healthDifference = maxHealth - oldMaxHealth;
                    curHealth += healthDifference;
                    curHealth = Mathf.Min(curHealth, maxHealth);

                    if (duration != -1)
                    {
                        yield return new WaitForSeconds(duration);
                        maxHealth_mul -= (int) value;
                        healthDifference = oldMaxHealth - maxHealth;
                        curHealth -= healthDifference;
                        curHealth = Mathf.Min(curHealth, maxHealth);
                    }
                    break;
                
                case "attackSpeed_add":
                    attackSpeed_add += value;
                    if (duration != -1)
                    {
                        yield return new WaitForSeconds(duration);
                        attackSpeed_add -= value;
                    }
                    break;
                
                case "attackSpeed_mul":
                    attackSpeed_mul += value;
                    if (duration != -1)
                    {
                        yield return new WaitForSeconds(duration);
                        attackSpeed_mul -= value;
                    }
                    break;
                
                case "attackDamage_add":
                    attackDamage_add += value;
                    if (duration != -1)
                    {
                        yield return new WaitForSeconds(duration);
                        attackDamage_add -= value;
                    }
                    break;
                
                case "attackDamage_mul":
                    attackDamage_mul += value;
                    if (duration != -1)
                    {
                        yield return new WaitForSeconds(duration);
                        attackDamage_mul -= value;
                    }
                    break;
            }
        }

        private void AddOrRemoveSkillAttributes(Skill skill, bool isRemove)
        {
            if (skill == null) return;
            if (skill.attributes == null) return;
            if (attackConfig == null) return;
            
            foreach (var kvp in skill.attributes)
            {
                var key = kvp.Key;
                var value = kvp.Value;
                switch (key)
                {
                    case "bulletForward":
                        attackConfig.forwardBulletCount = isRemove? 1 : 1 + (int) value;
                        break;
                    case "bulletBackward":
                        attackConfig.backwardBulletCount = isRemove? 0 : (int) value;
                        break;
                    case "bulletSide":
                        attackConfig.sideBulletsCount = isRemove? 0 : (int) value;
                        break;
                    case "bulletContinue":
                        attackConfig.continuousShots = isRemove? 1 : 1 + (int) value;
                        break;
                    case "reboundWall":
                        attackConfig.wallRebound = !isRemove;
                        break;
                    case "bulletEject":
                        attackConfig.eject = !isRemove;
                        break;
                    case "throughEnemy":
                        attackConfig.penetration = !isRemove;
                        break;
                    case "headShot":
                        attackConfig.headshot = isRemove ? 0 : value;
                        break;
                    case "attackAdd":
                        attackSpeed -= attackSpeed_add;
                        attackSpeed_add = isRemove ? 0 : value;
                        attackSpeed += attackSpeed_add;
                        break;
                    case "attackSpeedAdd":
                        attackDamage -= attackDamage_add;
                        attackDamage_add = isRemove ? 0 : value;
                        attackDamage += attackDamage_add;
                        break;
                }
            }
        }

        private void OnDrawGizmosSelected()
        {
            if (forwardAttackPoint && backwardAttackPoint && leftsideAttackPoint && rightsideAttackPoint)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawRay(forwardAttackPoint.position, forwardAttackPoint.forward);
                Gizmos.color = Color.green;
                Gizmos.DrawRay(backwardAttackPoint.position, backwardAttackPoint.forward);
                Gizmos.color = Color.blue;
                Gizmos.DrawRay(leftsideAttackPoint.position, leftsideAttackPoint.forward);
                Gizmos.color = Color.yellow;
                Gizmos.DrawRay(rightsideAttackPoint.position, rightsideAttackPoint.forward);
            }
        }

        protected virtual void OnDestroy()
        {
            StopAllCoroutines();
        }
    }
}
