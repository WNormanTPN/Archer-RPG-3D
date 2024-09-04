using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Config;
using DG.Tweening;
using Entity.Attack;
using Entity.Enemy;
using JetBrains.Annotations;
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

        private CharacterBase characterBase;
        private GameObject bulletPrefab;
        private GameObject destroyFX;
        private float bulletSpreadAngle = 90 / 5f;
        
        private string _destroyFXKey;
        private string _bulletPrefabKey;

        public Weapon()
        {
            attackLogics = new List<AttackLogic>();
        }

        public Weapon(int id)
        {
            var data = ConfigDataManager.Instance.GetConfigData<WeaponCollection>().Weapons[id.ToString()];
            weaponID = data.weaponID;
            ballistic = data.ballistic;
            attackLogics = data.attackLogics?.ToList();
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

        public void TriggerStartAttack(AttackConfig config)
        {
            if (ballistic == Ballistic.MeleeAttack)
            {
                GameObject bulletInstance = bulletPrefab? Object.Instantiate(bulletPrefab) : null;
                if (GetAttackLogic("DashBegin") != null)
                    DoDashAttack(config, bulletInstance);
            }
        }

        public void TriggerDoAttack(AttackConfig config)
        {
            if (ballistic == Ballistic.MeleeAttack)
            {
                if (GetAttackLogic("DashAttack") != null)
                    DoDashAttack(config);
            }
            else
            {
                if (characterBase == null && owner)
                {
                    characterBase = owner.GetComponent<CharacterBase>();
                }
                characterBase?.StartCoroutine(DoAttackCoroutine(config));
            }
        }
        
        private IEnumerator DoAttackCoroutine(AttackConfig config)
        {
            for (int i = 0; i < config.continuousShots; i++)
            {
                // Instantiate the bullet
                List<GameObject> bulletInstances = InstantiateBullets(config);

                if (destroyFX)
                {
                    config.destroyFX = destroyFX;
                }

                // Configure the bullet's movement based on ballistic type
                switch (ballistic)
                {
                    case Ballistic.BulletStraight:
                        foreach (var bulletInstance in bulletInstances)
                        {
                            bulletInstance.AddComponent<StraightMovement>().Init(speed, distance, attackLogics, config);
                        }

                        break;
                    case Ballistic.BulletStraightWithTrajectory:
                        foreach (var bulletInstance in bulletInstances)
                        {
                            bulletInstance.AddComponent<StraightMovement>().Init(speed, distance, attackLogics, config);
                        }
                        
                        break;
                    case Ballistic.BulletCurve:
                        foreach (var bulletInstance in bulletInstances)
                        {
                            bulletInstance.AddComponent<CurveMovement>().Init(speed, distance, attackLogics, config);
                        }

                        break;
                    case Ballistic.BulletParabola:
                        foreach (var bulletInstance in bulletInstances)
                        {
                            bulletInstance.AddComponent<ParabolaMovement>().Init(speed, distance, config.target);
                        }

                        break;
                    case Ballistic.BulletChase:
                        foreach (var bulletInstance in bulletInstances)
                        {
                            bulletInstance.AddComponent<ChaseMovement>().Init(speed, distance, config.target);
                        }

                        break;
                    case Ballistic.BulletRound:
                        foreach (var bulletInstance in bulletInstances)
                        {
                            bulletInstance.AddComponent<RoundMovement>().Init(speed, distance, 1f);
                        }

                        break;
                }
                yield return new WaitForSeconds(1f / speed);
            }
        }
        
        public void TriggerEndAttack(AttackConfig config)
        {
            return;
        }
        
        private void DoDashAttack(AttackConfig config, GameObject bulletInstance = null)
        {
            if (owner.TryGetComponent<Dash>(out var dash))
            {
                dash.vfx = bulletInstance;
                dash.config = config;
                dash.DoDash(distance, speed);
            }
            else
            {
                dash = owner.AddComponent<Dash>();
                dash.vfx = bulletInstance;
                dash.config = config;
                dash.DoDash(distance, speed);
            }
        }
        
        private List<GameObject> InstantiateBullets(AttackConfig config)
        {
            List<GameObject> bulletInstances = null;
            if (bulletPrefab)
            {
                bulletInstances = new List<GameObject>();
                InstantiateBulletsHelper(ref bulletInstances, config.forwardAttackPoint, config.forwardBulletCount);
                InstantiateBulletsHelper(ref bulletInstances, config.backwardAttackPoint, config.backwardBulletCount);
                InstantiateBulletsHelper(ref bulletInstances, config.leftsideAttackPoint, config.sideBulletsCount);
                InstantiateBulletsHelper(ref bulletInstances, config.rightsideAttackPoint, config.sideBulletsCount);
            }
            return bulletInstances;
        }
        
        private void InstantiateBulletsHelper(ref List<GameObject> bulletInstances, Transform attackPoint, int bulletCount)
        {
            var directions = CalculateDirectionOfBullets(attackPoint, bulletCount);
            for (int i = 0; i < bulletCount; i++)
            {
                var bullet = Object.Instantiate(bulletPrefab);
                bullet.transform.position = attackPoint.position;
                bullet.transform.rotation = Quaternion.LookRotation(directions[i]);
                bulletInstances.Add(bullet);
            }
        }
        
        public List<Vector3> CalculateDirectionOfBullets(Transform attackPoint, int bulletCount)
        {
            List<Vector3> directions = new List<Vector3>();
            var forward = attackPoint.forward;
            for (int i = 0; i < bulletCount; i++)
            {
                float angle = (i - (bulletCount - 1) / 2f) * bulletSpreadAngle;
                Quaternion rotation = Quaternion.Euler(0, angle, 0);
                Vector3 bulletDirection = rotation * forward;
                directions.Add(bulletDirection);
            }
            return directions;
        }
        
        private AttackLogic GetAttackLogic(string logic)
        {
            foreach (var attackLogic in attackLogics)
            {
                if (attackLogic.logic == logic)
                {
                    return attackLogic;
                }
            }
            return null;
        }
    }
    
    public interface IWeapon
    {
        void TriggerStartAttack(AttackConfig config = null);
        void TriggerDoAttack(AttackConfig config = null);
        void TriggerEndAttack(AttackConfig config = null);
    }
    
    [Serializable]
    public class AttackConfig
    {
        public float damage;
        public float knockback;
        public Transform target;
        public GameObject destroyFX;
        public Transform forwardAttackPoint;
        public Transform backwardAttackPoint;
        public Transform leftsideAttackPoint;
        public Transform rightsideAttackPoint;
        public int forwardBulletCount = 1;
        public int backwardBulletCount = 0;
        public int sideBulletsCount = 0;
        public int continuousShots = 1;
        public bool wallRebound = false;
        public bool eject = false;
        public bool penetration = false;
        public float headshot = 0;
    }

    [Serializable]
    public class WeaponCollection : IConfigCollection
    {
        public Dictionary<string, Weapon> Weapons;

        public WeaponCollection()
        {
            Weapons = new Dictionary<string, Weapon>();
        }
        
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

    [Serializable]
    public enum Ballistic
    {
        BulletStraight = 0,
        BulletStraightWithTrajectory = 1,
        BulletCurve = 2,
        BulletParabola = 3,
        BulletChase = 4,
        BulletRound = 5,
        MeleeAttack = 6,
    }
}
