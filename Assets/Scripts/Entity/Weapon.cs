using System;
using System.Collections.Generic;
using UnityEngine;

namespace Entity
{
    [Serializable]
    public class WeaponData
    {
        public int weaponID;
        public Ballistic ballistic;
        public BulletLogic bulletLogic;
        public float? distance;
        public float? speed;
        public float? knockback;
        public string destroyEffectKey;
    }
    
    [Serializable]
    public class WeaponDataCollection
    {
        public Dictionary<string, WeaponData> weaponDatas;
    }

    [Serializable]
    public class BulletLogic
    {
        public string logic;
        public Dictionary<string, float> args;
    }

    public enum Ballistic
    {
        Straight = 0,
        Curve = 1,
        Parabola = 2,
        Chase = 3,
        Round = 4
    }

}
