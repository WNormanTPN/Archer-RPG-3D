using Entity.Attack;
using UnityEngine;

namespace Entity.Player
{
    public class ArcherBehavior : PlayerController
    {
        [Header("Archer Settings")]
        public GameObject arrowPrefab;
        public float arrowSpeed = 15f;
        public float arrowLifeTime = 5f;
        public float shootingOffset = 0.3f;
        public float shootingAngle = 10f;

        public void ShootArrow()
        {
            Vector3 shootingDirection = Quaternion.AngleAxis(-shootingAngle, transform.right) * transform.forward;
            
            GameObject arrow = Instantiate(
                arrowPrefab,
                transform.position + shootingOffset * Vector3.up,
                Quaternion.LookRotation(shootingDirection)
                );
            
            Rigidbody rb = arrow.GetComponent<Rigidbody>();
            rb.velocity = shootingDirection * arrowSpeed;

            // Get the ArrowBehavior component and start the rotation correction coroutine
            ArrowBehavior arrowBehavior = arrow.GetComponent<ArrowBehavior>();
            arrowBehavior.StartRotationCorrection();

            // Destroy the arrow after the specified lifetime
            Destroy(arrow, arrowLifeTime);
        }
    }
}