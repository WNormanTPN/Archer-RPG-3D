using System;
using System.Collections;
using Entity.Attack;
using UnityEngine;

namespace Entity.Enemy
{
    public class SparcherController : EnemyController, IRangedAttack
    {
        [Header("Ranged Attack Settings")]
        public GameObject lineRenderer;                     // Line renderer for the shooting effect
        public float lineRendererWidth = 0.5f;              // Width of the line renderer
        
        private GameObject lineRendererInstance;

        protected override void FixedUpdate()
        {
            base.FixedUpdate();
            if (!lineRenderer) return;
            if (isAttacking)
            {
                // Draw trajectory line
                if (!lineRendererInstance)
                {
                    lineRendererInstance = Instantiate(lineRenderer);
                }

                var lineRendererComponent = lineRendererInstance.GetComponent<LineRenderer>();
                DrawTrajectoryLine(lineRendererComponent, attackPoint.position, attackPoint.forward);
            }
            else
            {
                if (lineRendererInstance)
                {
                    Destroy(lineRendererInstance);
                }
            }
        }
        
        void DrawTrajectoryLine(LineRenderer lineRenderer, Vector3 start, Vector3 direction)
        {
            Vector3 end = start + direction * 100f;
            Ray ray = new Ray(start, direction);
            RaycastHit[] hits = new RaycastHit[10];
            int size = Physics.RaycastNonAlloc(ray, hits, 100f, LayerMask.NameToLayer("Enemy"));
            
            foreach (var hit in hits)
            {
                if (hit.collider.gameObject.layer != LayerMask.NameToLayer("Enemy"))
                {
                    end = hit.point;
                    break;
                }
            }
            
            lineRenderer.startWidth = lineRendererWidth;
            lineRenderer.endWidth = lineRendererWidth;
            lineRenderer.SetPosition(0, start);
            lineRenderer.SetPosition(1, end);
        }

        public void ShootProjectile()
        {
            StopAttack();
            if (lineRendererInstance != null)
            {
                Destroy(lineRendererInstance);
            }

            weapon.DoAttack(attackConfig);
        }
    }
}