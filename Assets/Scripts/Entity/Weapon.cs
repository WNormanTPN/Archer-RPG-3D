using System;
using System.Collections.Generic;
using Config;
using DG.Tweening;
using Entity.Attack;
using Entity.Enemy;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Object = UnityEngine.Object;

namespace Entity
{
    [Serializable]
    public class Weapon : IWeapon
    {
        [JsonProperty("ID")]public int weaponID;
        public Ballistic ballistic;
        public List<AttackLogic> attackLogics;
        public float distance;
        public float speed;
        public float? knockback;
        [JsonProperty("destroyFX")] public string destroyFXKey
        {
            get => _destroyFXKey;
            set
            {
                if (_destroyFXKey != value)
                {
                    _destroyFXKey = value;
                    Init();
                }
            }
        }
        [JsonProperty("bulletKey")] public string bulletPrefabKey
        {
            get => _bulletPrefabKey;
            set
            {
                if (_bulletPrefabKey != value)
                {
                    _bulletPrefabKey = value;
                    Init();
                }
            }
        }
        public GameObject owner;

        private GameObject bulletPrefab;
        private GameObject destroyFX;
        
        private string _destroyFXKey;
        private string _bulletPrefabKey;
        
        public Weapon() {}

        public Weapon(int id)
        {
            var data = ConfigDataManager.Instance.GetConfigData<WeaponCollection>().Weapons[id.ToString()];
            weaponID = data.weaponID;
            ballistic = data.ballistic;
            attackLogics = data.attackLogics;
            distance = data.distance;
            speed = data.speed;
            knockback = data.knockback;
            destroyFXKey = data.destroyFXKey;
            bulletPrefabKey = data.bulletPrefabKey;
            Init();
        }

        public void Init()
        {
            if (!string.IsNullOrEmpty(bulletPrefabKey) && bulletPrefab == null)
            {
                var loadBulletTask = Addressables.LoadAssetAsync<GameObject>(bulletPrefabKey);
                loadBulletTask.Completed += handle => { bulletPrefab = handle.Result; };
            }

            if (!string.IsNullOrEmpty(destroyFXKey) && destroyFX == null)
            {
                var loadDestroyFXTask = Addressables.LoadAssetAsync<GameObject>(destroyFXKey);
                loadDestroyFXTask.Completed += handle => { destroyFX = handle.Result; };
            }
        }

        public GameObject DoAttack(AttackConfig config)
        {
            // Instantiate the bullet
            GameObject bulletInstance = bulletPrefab? Object.Instantiate(bulletPrefab) : null;
            
            if (destroyFX)
            {
                config.destroyFX = destroyFX;
            }

            // Configure the bullet's movement based on ballistic type
            switch (ballistic)
            {
                case Ballistic.BulletStraight:
                    bulletInstance.AddComponent<StraightMovement>().Init(speed, distance, attackLogics, config);
                    break;
                case Ballistic.BulletCurve:
                    bulletInstance.AddComponent<CurveMovement>().Init(speed, distance, attackLogics, config);
                    break;
                case Ballistic.BulletParabola:
                    bulletInstance.AddComponent<ParabolaMovement>().Init(speed, distance, attackLogics, config);
                    break;
                case Ballistic.BulletChase:
                    bulletInstance.AddComponent<ChaseMovement>().Init(speed, distance, attackLogics, config);
                    break;
                case Ballistic.BulletRound:
                    bulletInstance.AddComponent<RoundMovement>().Init(speed, distance, attackLogics, config);
                    break;
                case Ballistic.MeleeDash:
                    if (owner.TryGetComponent<Dash>(out var dash))
                    {
                        dash.vfx = bulletPrefab? bulletInstance : null;
                        dash.config = config;
                        dash.DoDash(distance, speed);
                    }
                    else
                    {
                        dash = owner.AddComponent<Dash>();
                        dash.vfx =  bulletPrefab? bulletInstance : null;
                        dash.config = config;
                        dash.DoDash(distance, speed);
                    }
                    break;
            }

            return null;
        }
    }
    
    public interface IWeapon
    {
        GameObject DoAttack(AttackConfig config = null);
    }
    
    public class AttackConfig
    {
        public float damage;
        public float knockback;
        public Transform from;
        public Transform to;
        public GameObject destroyFX;
    }

    [Serializable]
    public class WeaponCollection : IConfigCollection
    {
        public Dictionary<string, Weapon> Weapons;
        
        public WeaponCollection() {}
        
        [JsonConstructor]
        public WeaponCollection(Dictionary<string, Weapon> Weapons)
        {
            this.Weapons = Weapons;
        }

        public void FromJson(string json)
        {
            Weapons = JsonConvert.DeserializeObject<Dictionary<string, Weapon>>(json);
        }
    }

    [Serializable]
    public class AttackLogic
    {
        public string logic;
        public Dictionary<string, float> args;
    }

    public enum Ballistic
    {
        BulletStraight = 0,
        BulletCurve = 1,
        BulletParabola = 2,
        BulletChase = 3,
        BulletRound = 4,
        MeleeDash = 5,
    }

    // Movement behavior scripts

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
            
            if (config.from)
            {
                transform.position = config.from.position;
                transform.rotation = config.from.rotation;
            }
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
                direction = config.from?.forward ?? transform.forward;
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
