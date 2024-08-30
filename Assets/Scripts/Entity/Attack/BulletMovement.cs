using System.Collections.Generic;
using UnityEngine;

namespace Entity.Attack
{
    public abstract class BulletMovement : MonoBehaviour
    {
        public float speed;
        public float distance;
        public List<AttackLogic> attackLogics;
        public AttackConfig config;
        public Rigidbody rb;

        public virtual void Init(float speed, float distance, List<AttackLogic> attackLogics, AttackConfig config)
        {
            this.speed = speed;
            this.distance = distance;
            this.config = config;
            this.attackLogics = attackLogics;
            rb = GetComponent<Rigidbody>();
        }

        protected virtual void Update()
        {
            if (rb.isKinematic) return;
        }
    }

    public class StraightMovement : BulletMovement
    {
        private Vector3 direction;
        
        protected override void Update()
        {
            base.Update();
            if (direction == Vector3.zero)
            {
                direction = transform.forward;
            }
            rb.velocity = speed * direction;
            distance -= rb.velocity.magnitude * Time.deltaTime;

            if (distance <= 0)
            {
                Destroy(gameObject);
            }
        }
    }


    public class CurveMovement : BulletMovement
    {
        private float curveSpeed = 5f;

        public void Init(float speed, float distance, float curveSpeed)
        {
            base.Init(speed, distance, attackLogics, config);
            this.curveSpeed = curveSpeed;
        }

        protected override void Update()
        {
            base.Update();
            // Curve using sinusoidal wave pattern
            float x = speed * Time.deltaTime;
            float y = Mathf.Sin(Time.time * curveSpeed) * 0.5f; // Adjust amplitude if needed
            float z = speed * Time.deltaTime;

            transform.Translate(new Vector3(x, y, z));
            distance -= speed * Time.deltaTime;

            if (distance <= 0)
            {
                Destroy(gameObject);
            }
        }
    }


    public class ParabolaMovement : BulletMovement
    {
        private Transform target;
        private Vector3 startPosition;
        private float flightDuration;
        private float elapsedTime = 0f;

        public void Init(float speed, float distance, Transform target)
        {
            base.Init(speed, distance, attackLogics, config);
            this.target = target;
            startPosition = transform.position;
            flightDuration = distance / speed;
        }

        protected override void Update()
        {
            base.Update();
            if (!target) return;

            elapsedTime += Time.deltaTime;

            float progress = elapsedTime / flightDuration;
            Vector3 currentPos = Vector3.Lerp(startPosition, target.position, progress);

            // Add parabolic curve by modifying the y-axis
            float height = Mathf.Sin(Mathf.PI * progress) * (distance / 4); // Adjust for a higher arc
            currentPos.y += height;

            transform.position = currentPos;

            if (progress >= 1f)
            {
                Destroy(gameObject);
            }
        }
    }


    public class ChaseMovement : BulletMovement
    {
        private Transform target;

        public void Init(float speed, float distance, Transform target)
        {
            base.Init(speed, distance, attackLogics, config);
            this.target = target;
        }

        protected override void Update()
        {
            base.Update();
            if (target)
            {
                transform.position = Vector3.MoveTowards(transform.position, target.position, speed * Time.deltaTime);
                distance -= speed * Time.deltaTime;

                if (distance <= 0)
                {
                    Destroy(gameObject);
                }
            }
        }
    }


    public class RoundMovement : BulletMovement
    {
        private float rotationSpeed = 360f; // degrees per second
        private float radius = 1f;
        private Vector3 centerPosition;

        public void Init(float speed, float distance, float radius)
        {
            base.Init(speed, distance, attackLogics, config);
            this.radius = radius;
            centerPosition = transform.position;
        }

        protected override void Update()
        {
            base.Update();
            float angle = rotationSpeed * Time.time * Mathf.Deg2Rad;
            Vector3 offset = new Vector3(Mathf.Sin(angle), 0, Mathf.Cos(angle)) * radius;
            transform.position = centerPosition + offset;
            distance -= speed * Time.deltaTime;

            if (distance <= 0)
            {
                Destroy(gameObject);
            }
        }
    }
}