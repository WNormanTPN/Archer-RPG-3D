using DG.Tweening;
using Entity.Enemy;
using UnityEngine;

namespace Entity.Attack
{
    public class Dash : MonoBehaviour
    {
        public GameObject vfx;
        public AttackConfig config;
        
        public void DoDash(float distance, float speed)
        {
            var ownerController = gameObject.GetComponent<EnemyController>();
            var direction = gameObject.transform.forward;
            var time = 1 / speed;
            if (vfx != null)
            {
                vfx.transform.parent = gameObject.transform;
                vfx.transform.position = gameObject.transform.position;
                vfx.transform.rotation = gameObject.transform.rotation;
            }
            ownerController.ForceStopRotate();

            var tweener = gameObject.transform.DOMove(gameObject.transform.position + direction * distance, time);
            tweener.OnComplete(() =>
            {
                ownerController.ActivateRotate();
                Destroy(vfx);
                if (config.destroyFX != null)
                {
                    var pos = config.forwardAttackPoint.position;
                    var rot = config.destroyFX.transform.rotation;
                    var destroyFX = Instantiate(config.destroyFX, pos, rot, config.forwardAttackPoint);
                    Destroy(destroyFX, 1f);
                }
            });
    }
    }
}