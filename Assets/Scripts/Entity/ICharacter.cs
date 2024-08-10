using UnityEngine;

namespace Entity
{
    public interface ICharacter
    {
        int exp { get; set; }
    
        void Move(Vector3 direction);
        void StopMove();
        void Rotate(Vector3 direction);
        void Attack();
        void StopAttack();
        void SetScale(float scale);
        void SetWeapon(Weapon weapon);
        void SetSkill(Skill skill);
    }
}
