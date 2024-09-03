using System;
using System.Collections.Generic;
using Newtonsoft.Json;
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
    public interface ICharacter
    {
        void SetUpCharacter(CharacterData data);
        void Move(Vector3 direction);
        void StopMove();
        void Rotate(Vector3 direction);
        void StartAttackAnim();
        void StopAttack();
        void SetScale(float scale);
        void TakeDamage(float damage);
        void Die();
        void AddSkill(int skillID);
        void AddSkill(Skill skill);
        void AddEffect(int effectID);
        void AddEffect(Effect effect);
    }
}
