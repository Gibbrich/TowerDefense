using UnityEngine;
using System.Collections;
using Game;
using Gamelogic.Extensions;

public class SimpleTower : BaseTower
{
    #region Editor tweakable fields

    [SerializeField] private GuidedProjectile projectilePrefab;

    #endregion

    #region Private methods

    protected override void Shoot(Monster monster)
    {
        GuidedProjectile projectile = pool.GetNewObjectSilently(10) as GuidedProjectile;
        projectile.m_target = monster.gameObject;
    }

    protected override bool CheckDistance(Monster monster)
    {
        return Vector3.Distance(transform.position, monster.transform.position) <= shootRange;
    }

    protected override BaseProjectile GetProjectilePrefab()
    {
        return projectilePrefab;
    }

    #endregion
}