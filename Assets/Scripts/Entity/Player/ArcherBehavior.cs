using System.Collections.Generic;
using Entity.Attack;
using MyEditor;
using UnityEngine;

namespace Entity.Player
{
    public class ArcherBehavior : PlayerController, IRangedAttack
    {
        [InspectorGroup("Attack Settings")]
        public float attackDamage = 10f;
        
        protected override void Start()
        {
            base.Start();
            curHealth = maxHealth;
            attackConfig.damage = attackDamage;
        }

        public void ShootProjectile()
        {
            weapon.DoAttack(attackConfig);
        }
    }
}