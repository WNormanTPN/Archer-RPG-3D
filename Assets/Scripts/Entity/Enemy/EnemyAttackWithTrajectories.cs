using System.Collections.Generic;
using Generic;
using UnityEngine;

namespace Entity.Enemy
{
    public class EnemyAttackWithTrajectories : EnemyController
    {
        [Header("Trajectory Settings")]
        public GameObject lineRendererPrefab;  // Prefab for the line renderer
        public float lineRendererWidth = 0.5f; // Width of the line renderer

        private List<LineRenderer> lineRendererComponents;
        private ObjectPool objectPool;          // Reference to the object pool
        private bool isAttackTriggered = false; // Flag to track if attack has been triggered

        protected override void FixedUpdate()
        {
            base.FixedUpdate();
            if (!lineRendererPrefab) return;

            if (!objectPool)
            {
                objectPool = FindObjectOfType<ObjectPool>();
            }

            if (isAttacking)
            {
                if (!isAttackTriggered)
                {
                    // Get line renderers from the pool only once when attacking starts
                    GetLineRenderersFromPool();
                    isAttackTriggered = true;
                }
                UpdateTrajectoryLines();
            }
            else
            {
                // Return all line renderers to the pool
                ReturnAllLineRenderersToPool();
                isAttackTriggered = false;
            }
        }

        void GetLineRenderersFromPool()
        {
            lineRendererComponents = new List<LineRenderer>();
            int totalLineRenderers = attackConfig.forwardBulletCount +
                                     attackConfig.backwardBulletCount +
                                     attackConfig.sideBulletsCount * 2; // Assuming sideBulletsCount is for both sides

            for (int i = 0; i < totalLineRenderers; i++)
            {
                var lineRendererInstance = objectPool.GetObject(lineRendererPrefab);
                var lineRendererComponent = lineRendererInstance.GetComponent<LineRenderer>();
                lineRendererComponent.startWidth = lineRendererWidth;
                lineRendererComponent.endWidth = lineRendererWidth;
                lineRendererComponents.Add(lineRendererComponent);
            }
        }

        void UpdateTrajectoryLines()
        {
            var directions = weapon.CalculateDirectionOfBullets(forwardAttackPoint, attackConfig.forwardBulletCount);
            int lineIndex = 0;

            // Update forward trajectory lines
            UpdateTrajectoryLines(forwardAttackPoint, attackConfig.forwardBulletCount, ref lineIndex);
            UpdateTrajectoryLines(backwardAttackPoint, attackConfig.backwardBulletCount, ref lineIndex);
            UpdateTrajectoryLines(rightsideAttackPoint, attackConfig.sideBulletsCount, ref lineIndex);
            UpdateTrajectoryLines(leftsideAttackPoint, attackConfig.sideBulletsCount, ref lineIndex);
        }

        void UpdateTrajectoryLines(Transform start, int count, ref int lineIndex)
        {
            var directions = weapon.CalculateDirectionOfBullets(start, count);

            for (int i = 0; i < count; i++)
            {
                var direction = directions[i];
                Vector3 end = start.position + direction * weapon.distance;
                Ray ray = new Ray(start.position, direction);
                RaycastHit[] hits = new RaycastHit[10];
                int size = Physics.RaycastNonAlloc(ray, hits, weapon.distance);

                for (int j = size - 1; j >= 0; j--)
                {
                    var hit = hits[j];
                    if (hit.collider?.gameObject.layer != LayerMask.NameToLayer("Enemy"))
                    {
                        end = hit.point;
                        break;
                    }
                }

                // Update the position of the line renderer
                if (lineIndex < lineRendererComponents.Count)
                {
                    var lineRendererComponent = lineRendererComponents[lineIndex];
                    lineRendererComponent.SetPosition(0, start.position);
                    lineRendererComponent.SetPosition(1, end);
                    lineIndex++;
                }
            }
        }

        void ReturnAllLineRenderersToPool()
        {
            if (lineRendererComponents != null)
            {
                foreach (var lineRendererComponent in lineRendererComponents)
                {
                    if (!lineRendererComponent) continue;
                    var lineRendererInstance = lineRendererComponent.gameObject;
                    if (!objectPool)
                    {
                        objectPool.ReturnObject(lineRendererPrefab, lineRendererInstance);
                    }
                    else
                    {
                        Destroy(lineRendererInstance);
                    }
                }
                lineRendererComponents.Clear();
            }
        }

        public override void TriggerDoAttack()
        {
            base.TriggerDoAttack();
            StopAttack();
            // Return all line renderers to the pool
            ReturnAllLineRenderersToPool();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            // Return all line renderers to the pool
            ReturnAllLineRenderersToPool();
        }
    }
}
