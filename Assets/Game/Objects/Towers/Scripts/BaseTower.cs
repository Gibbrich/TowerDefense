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
    
    [Tooltip("Периодичность стрельбы, выстрел/сек")]
    [SerializeField] protected float shootInterval = 0.5f;
    [SerializeField] protected float shootRange = 4f;
    [SerializeField] protected GameObject shootSocket;
    
    #endregion
    
    #region Private fields

    protected float lastShotTime;
    protected List<Monster> monsters;
    protected Pool<BaseProjectile> pool;

    private GameObject projectilesParent;

    #endregion
    
    #region Unity callbacks

    protected virtual void Start()
    {
        monsters = new List<Monster>();

        projectilesParent = GameObject.Find("Projectiles");
        if (projectilesParent == null)
        {
            projectilesParent = new GameObject("Projectiles");
        }

        SphereCollider sphereCollider = gameObject.AddComponent<SphereCollider>();
        sphereCollider.radius = shootRange;
        
        pool = new Pool<BaseProjectile>(10,
                                        CreateProjectile,
                                        projectile => Destroy(projectile.gameObject),
                                        ProjectileWakeUp,
                                        projectile => projectile.gameObject.SetActive(false));
    }
    
    protected virtual void OnTriggerEnter(Collider other)
    {
        Monster monster = other.GetComponent<Monster>();
        if (monster != null)
        {
            monsters.Add(monster);
            monster.Death += OnMonsterDeath;
        }
    }

    protected virtual void OnTriggerExit(Collider other)
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
        projectile.Refresh();
        projectile.gameObject.SetActive(true);
    }
    
    private void OnMonsterDeath(Monster monster)
    {
        monsters.Remove(monster);
    }

    protected abstract BaseProjectile GetProjectilePrefab();

    protected virtual BaseProjectile CreateProjectile()
    {
        return BaseProjectile.Create(GetProjectilePrefab(), shootSocket.transform.position, projectilesParent.transform,
                                     this);
    }

    #endregion
}