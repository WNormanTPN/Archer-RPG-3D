using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ICharacter
{
    void Move(Vector3 direction);
    void StopMove();
    void Rotate(Vector3 direction);
    void Attack();
    void StopAttack();
}
