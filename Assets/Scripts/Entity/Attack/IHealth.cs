using UnityEngine;

namespace Entity.Attack
{
    public interface IHealth
    {
        float curHealth { get; set; }
        float maxHealth { get; set; }
    }
}