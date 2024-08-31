using UnityEngine;

namespace Entity
{
    public interface ICharacter
    {
        void Move(Vector3 direction);
        void StopMove();
        void Rotate(Vector3 direction);
        void StartAttackAnim();
        void StopAttack();
        void SetScale(float scale);
        void TakeDamage(float damage);
        void Die();
    }
}
