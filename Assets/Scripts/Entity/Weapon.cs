using System.Collections.Generic;
using UnityEngine;

namespace Entity
{
    public class Weapon : MonoBehaviour
    {
        public int weaponID;
        public Ballistic ballistic;
        public BulletLogic bulletLogic;
        public float distance;
        public float speed;
        public float knockback;
        public GameObject destroyEffect;
    }
    
    public enum Ballistic
    {
        Straight = 0,
        Curve = 1,
        Parabola = 2,
        Chase = 3,
        Round = 4
    }

    public struct BulletLogic
    {
        public string logicName;
        public Dictionary<string, float> args;
    }
}
