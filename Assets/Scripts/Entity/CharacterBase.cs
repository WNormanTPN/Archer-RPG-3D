using System;
using System.Collections.Generic;
using System.Linq;
using Config;
using MyEditor;
using Newtonsoft.Json;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;

namespace Entity
{
    [Serializable]
    public class CharacterData
    {
        [JsonProperty("charID")]
        public int id;
    
        [JsonProperty("charName")]
        public string name;
    
        public float scale = 1;
    
        [JsonProperty("weaponID")]
        public int weaponId;
    
        [JsonProperty("skills")]
        public List<int> skillIds;
    
        public int exp;
    
        [JsonProperty("key")]
        public string prefabKey;
    }

    
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
        [Range(0, 10)] public float moveSpeed = 5f;           // Speed of the character movement
        [Range(0, 720)] public float rotationSpeed = 720f;    // Speed of the character rotation in degrees per second
        
        [NonGroup]
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
        public SkillCollection skills;                        // List of skills the character has
        public List<Effect> effects;
        
        [LastGroup]
        public CharacterData characterInitData;               // Initial data of the character
    
        [InspectorGroup("Attack Settings")]
        [Range(0, 1000)]public int maxHealth = 100;           // Health of the character
        [Range(0, 1000)] public int curHealth = 100;          // Current health of the character
        [Range(0, 10)] public float attackSpeed = 1f;         // Speed of the character attack per second
        [Range(0, 1000)] public float attackDamage = 10f;     // Damage dealt by the character
        [FormerlySerializedAs("attackPoint")] public Transform forwardAttackPoint;                         // Point where the attack will be executed
        
        protected Animator animator;                          // Reference to the Animator component
        protected float velocity;                             // Current velocity of the character
        protected Rigidbody rb;                               // Reference to the Rigidbody component
        protected AttackConfig attackConfig;                  // Configuration of the attack
        protected float addedAttackSpeed = 0;                 // Added attack speed from skills
        protected float addedAttackDamage = 0;                // Added attack damage from skills
        protected Transform backwardAttackPoint;              // Point where the backward attack will be executed
        protected Transform leftsideAttackPoint;              // Point where the left attack will be executed
        protected Transform rightsideAttackPoint;             // Point where the right attack will be executed
        

        protected virtual void Awake()
        {
            
        }

        protected virtual void Start()
        {
            rb = GetComponent<Rigidbody>();
            animator = GetComponent<Animator>();
            attackConfig = new AttackConfig();
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

        private void CalculateAttackPoints()
        {
            var direction = forwardAttackPoint.position - transform.position;
            backwardAttackPoint = GameObject.Instantiate(forwardAttackPoint, transform);
            leftsideAttackPoint = GameObject.Instantiate(forwardAttackPoint, transform);
            rightsideAttackPoint = GameObject.Instantiate(forwardAttackPoint, transform);
            direction = Quaternion.Euler(0, 180, 0) * direction;
            backwardAttackPoint.position = transform.position + direction;
            backwardAttackPoint.rotation = Quaternion.Euler(0, 180, 0);
            direction = Quaternion.Euler(0, 90, 0) * direction;
            leftsideAttackPoint.position = transform.position + direction;
            leftsideAttackPoint.rotation = Quaternion.Euler(0, -90, 0);
            direction = Quaternion.Euler(0, -180, 0) * direction;
            rightsideAttackPoint.position = transform.position + direction;
            rightsideAttackPoint.rotation = Quaternion.Euler(0, 90, 0);
        }
        
        protected virtual void LoadInitData()
        {
            SetScale(characterInitData.scale);
            if (characterInitData.weaponId != 0 && weapon?.weaponID != characterInitData.weaponId)
            {
                weapon = new Weapon(characterInitData.weaponId);
            }
            if (characterInitData.skillIds is { Count: > 0 })
            {
                foreach (var skillId in characterInitData.skillIds)
                {
                    var skill = new Skill(skillId);
                    AddSkill(skill);
                    foreach (var exclusion in skill.exclusions)
                    {
                        var exclusionSkill = ConfigDataManager.Instance.GetConfigData<SkillCollection>().Skills[exclusion.ToString()];
                        AddOrRemoveSkillAttributes(exclusionSkill, true);
                    }
                }
            }
        }
        
        protected virtual void AddSkill(Skill skill)
        {
            skill = skills.AddSkill(skill);
            AddOrRemoveSkillAttributes(skill, false);
        }

        private void AddOrRemoveSkillAttributes(Skill skill, bool isRemove)
        {
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
                        attackSpeed -= addedAttackSpeed;
                        addedAttackSpeed = isRemove ? 0 : value;
                        attackSpeed += addedAttackSpeed;
                        break;
                    case "attackSpeedAdd":
                        attackDamage -= addedAttackDamage;
                        addedAttackDamage = isRemove ? 0 : value;
                        attackDamage += addedAttackDamage;
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
    }
}
