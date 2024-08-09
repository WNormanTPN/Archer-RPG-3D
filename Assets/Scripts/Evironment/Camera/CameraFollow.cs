using UnityEngine;

namespace Environment.Camera
{
    public class CameraFollow : MonoBehaviour
    {
        public Transform target;           // The target (player) to follow
        public float height = 10f;         // Height above the target
        public Vector3 rotation = new Vector3(45, 0, 0); // Rotation of the camera

        void LateUpdate()
        {
            if (target)
            {
                // Convert the rotation to a quaternion
                Quaternion rotationQuaternion = Quaternion.Euler(rotation);

                // Calculate the offset from the target
                Vector3 offset = new Vector3(0, height, 0);

                // Apply rotation to the offset
                Vector3 rotatedOffset = rotationQuaternion * offset;

                // Calculate the camera position
                Vector3 targetPosition = target.position;
                transform.position = targetPosition + rotatedOffset;

                // Ensure the camera looks at the target
                transform.LookAt(targetPosition);
            }
            else
            {
                target = GameObject.FindGameObjectWithTag("Player").transform;
            }
        }
    }
}