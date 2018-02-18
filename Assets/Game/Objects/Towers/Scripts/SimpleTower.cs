using UnityEngine;
using System.Collections;
using System.Linq;
using Game;
using Gamelogic.Extensions;

public class SimpleTower : BaseTower
{
    #region Editor tweakable fields

    [SerializeField] private GuidedProjectile projectilePrefab;

    #endregion
    
    #region Unity callbacks

    private void Update()
    {
        if (GetProjectilePrefab() == null)
        {
            return;
        }

        if (Time.time - lastShotTime >= shootInterval)
        {
            foreach (Monster monster in monsters)
            {
                if (Vector3.Distance(transform.position, monster.transform.position) <= shootRange)
                {
                    GuidedProjectile projectile = pool.GetNewObjectSilently() as GuidedProjectile;
                    projectile.SetTarget(monster.gameObject);
                    
                    lastShotTime = Time.time;
                    break;
                }
            }
        }
    }

    #endregion

    #region Private methods

    protected override BaseProjectile GetProjectilePrefab()
    {
        return projectilePrefab;
    }

    #endregion
}