using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Game;
using Gamelogic.Extensions;
using ModestTree;
using UnityEngine;

public abstract class BaseTower : MonoBehaviour
{
    #region Editor tweakable fields
    
    [SerializeField] protected float shootInterval = 0.5f;
    [SerializeField] protected float shootRange = 4f;
    [SerializeField] private GameObject shootSocket;
    
    #endregion
    
    #region Private fields

    private float lastShotTime;
    
    /* todo    - think on better collection
     * @author - Dvurechenskiyi
     * @date   - 13.02.2018
     * @time   - 17:33
    */
    private List<Monster> monsters;

    private GameObject projectilesParent;
    
    protected Pool<BaseProjectile> pool;

    #endregion
    
    #region Unity callbacks

    protected virtual void Start()
    {
        monsters = new List<Monster>();

        projectilesParent = new GameObject("Projectiles");
        projectilesParent.transform.parent = transform;

        SphereCollider sphereCollider = gameObject.AddComponent<SphereCollider>();
        sphereCollider.radius = shootRange;
        
        pool = new Pool<BaseProjectile>(10,
                                        () => BaseProjectile.Create(GetProjectilePrefab(), shootSocket.transform.position, projectilesParent.transform, this),
                                        projectile => Destroy(projectile.gameObject),
                                        ProjectileWakeUp,
                                        projectile => projectile.gameObject.SetActive(false));
    }

    protected virtual void Update()
    {
        if (GetProjectilePrefab() == null)
        {
            return;
        }

        Monster monster = monsters
            .SkipWhile(_ => Time.time - lastShotTime < shootInterval)
            .Where(CheckDistance)
            .FirstOrDefault();

        if (monster != null)
        {
            Shoot(monster);
            lastShotTime = Time.time;
        }
    }
    
    private void OnTriggerEnter(Collider other)
    {
        Monster monster = other.GetComponent<Monster>();
        if (monster != null)
        {
            monsters.Add(monster);
            monster.Death += OnMonsterDeath;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        Monster monster = other.GetComponent<Monster>();
        if (monster != null)
        {
            monsters.Remove(monster);
            monster.Death -= OnMonsterDeath;
        }
    }

    private void OnDestroy()
    {
        monsters.ForEach(monster => monster.Death -= OnMonsterDeath);
    }

    #endregion
    
    #region Public methods

    public void ReleaseProjectile(BaseProjectile projectile)
    {
        pool.Release(projectile);
    }
    
    #endregion
    
    #region Private methods

    private void ProjectileWakeUp(BaseProjectile projectile)
    {
        projectile.transform.position = shootSocket.transform.position;
        projectile.gameObject.SetActive(true);
    }
    
    private void OnMonsterDeath(Monster monster)
    {
        monsters.Remove(monster);
    }

    protected abstract void Shoot(Monster monster);

    protected abstract bool CheckDistance(Monster monster);

    protected abstract BaseProjectile GetProjectilePrefab();

    #endregion
}