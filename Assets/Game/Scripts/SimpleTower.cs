using UnityEngine;
using System.Collections;
using Game;
using Gamelogic.Extensions;

public class SimpleTower : BaseTower
{
    #region Editor tweakable fields

    [SerializeField] protected GuidedProjectile projectilePrefab;

    #endregion

    #region Private fields

    private Pool<GuidedProjectile> pool;
    
    #endregion

    #region Unity callbacks

    protected override void Start()
    {
        base.Start();

        pool = new Pool<GuidedProjectile>(10,
                                          () => Instantiate(projectilePrefab, transform.position + Vector3.up * 1.5f, Quaternion.identity),
                                          projectile => Destroy(projectile.gameObject),
                                          projectile => projectile.gameObject.SetActive(true),
                                          projectile => projectile.gameObject.SetActive(false));
    }

    #endregion

    #region Private methods

    protected override void Shoot(Monster monster)
    {
        GuidedProjectile projectile = pool.GetNewObjectSilently(10);
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