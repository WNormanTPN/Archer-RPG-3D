using System;
using System.Collections.Generic;
using System.Linq;
using Config;
using MyEditor;
using Newtonsoft.Json;
using UnityEditor;
using UnityEngine;

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
        
        public CharacterDataCollection() {}
        
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

        // [NonGroup]
        public Weapon weapon
        {
            get => _weapon;
            set
            {
                _weapon = value;
                _weapon.owner = gameObject;
            }
        } // Reference to the weapon
        public List<Skill> skills;                            // List of skills the character has
        public List<Effect> effects;
        
        [LastGroup]
        public CharacterData characterInitData;               // Initial data of the character
    
        [InspectorGroup("Attack Settings")]
        [Range(0, 1000)]public int maxHealth = 100;           // Health of the character
        [Range(0, 1000)] public int curHealth = 100;          // Current health of the character
        [Range(0, 10)] public float attackSpeed = 1f;         // Speed of the character attack per second
        [Range(0, 1000)] public float attackDamage = 10f;     // Damage dealt by the character
        public Transform attackPoint;                         // Point where the attack will be executed
        
        protected Animator animator;                          // Reference to the Animator component
        protected float velocity;                             // Current velocity of the character
        protected Rigidbody rb;                               // Reference to the Rigidbody component
        protected AttackConfig attackConfig;                  // Configuration of the attack
        
        
        private Weapon _weapon;
        private List<Skill> _skills;
        private List<Effect> _effects;

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
            if (attackPoint)
            {
                attackConfig.from = attackPoint;
                attackPoint.rotation = transform.rotation;
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

        public abstract void StartAttack();
        public virtual void DoAttack()
        {
            weapon.DoAttack(attackConfig);
        }
        public abstract void StopAttack();

        public virtual void SetScale(float scale)
        {
            transform.localScale = new Vector3(scale, scale, scale);
        }
        
        protected virtual void LoadInitData()
        {
            SetScale(characterInitData.scale);
            if (characterInitData.weaponId != 0 && weapon?.weaponID != characterInitData.weaponId)
            {
                weapon = new Weapon(characterInitData.weaponId);
            }
        }
    }
}
