using System;
using Entity.Attack;
using UnityEngine;

namespace Entity.Enemy
{
    public class MeleeAttackEnemy : EnemyController, IMeleeAttack
    {
        [Header("Melee Attack Settings")]
        public Transform attackPoint;                       // Reference to the attack point
        [Range(0, 10)] public float attackRange = 0.25f;    // Range of the attack
        public LayerMask playerLayer;                       // Layer mask for the player

        private void Awake()
        {
            if (playerLayer == 0)
            {
                playerLayer = LayerMask.GetMask("Player");
            }
        }

        public void DoAttack()
        {
            Collider[] hitPlayers = Physics.OverlapSphere(attackPoint.position, attackRange, playerLayer);
            foreach (Collider player in hitPlayers)
            {
                Debug.Log("Player hit!");
            }
        }

        public void OnDrawGizmosSelected()
        {
            if (attackPoint == null)
            {
                return;
            }
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(attackPoint.position, attackRange);
        }
    }
}