using System;
using UnityEngine;

namespace Entity.Attack
{
    public class MeleeAttack : MonoBehaviour
    {
        private GameObject attackFX;
        private GameObject destroyFX;
        private float damage;
        private bool isAttacking = false;
        private Collider attackCollider;
        private LayerMask enemyLayer;
        
        void Start()
        {
            attackCollider = GetComponent<Collider>();
            attackCollider.enabled = false;
            enemyLayer = LayerMask.GetMask("Player");
        }
        
        public void StartAttack(AttackConfig config, GameObject fx)
        {
            if (!isAttacking)
            {
                isAttacking = true;
                attackCollider.enabled = true;
                damage = config.damage;
                destroyFX = config.destroyFX;
                if (fx != null)
                {
                    attackFX = Instantiate(fx, transform);
                }
            }
        }
        
        public void EndAttack()
        {
            if (isAttacking)
            {
                isAttacking = false;
                attackCollider.enabled = false;
                if (attackFX != null)
                {
                    Destroy(attackFX);
                }

                if (destroyFX != null)
                {
                    var newDestroyFX = Instantiate(destroyFX, transform.position, transform.rotation);
                    Destroy(newDestroyFX, 1f);
                }
            }
        }

        public void OnCollisionEnter(Collision other)
        {
            if (isAttacking && enemyLayer == other.gameObject.layer)
            {
                var character = other.gameObject.GetComponent<CharacterBase>();
                if (character != null)
                {
                    character.TakeDamage(damage);
                }
            }
        }
    }
}