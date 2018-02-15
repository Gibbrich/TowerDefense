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
    
    #endregion
    
    #region Private fields

    private float lastShotTime;
    
    /* todo    - think on better collection
     * @author - Dvurechenskiyi
     * @date   - 13.02.2018
     * @time   - 17:33
    */
    private List<Monster> monsters;

    protected GameObject projectilesParent;

    #endregion
    
    #region Unity callbacks

    protected virtual void Start()
    {
        monsters = new List<Monster>();

        projectilesParent = new GameObject("Projectiles");
        projectilesParent.transform.parent = transform;

        SphereCollider sphereCollider = gameObject.AddComponent<SphereCollider>();
        sphereCollider.radius = shootRange;
    }

    private void Update()
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
        }
    }

    private void OnTriggerExit(Collider other)
    {
        Monster monster = other.GetComponent<Monster>();
        if (monster != null)
        {
            monsters.Remove(monster);
        }
    }
    
    #endregion
    
    #region Public methods
    
    public abstract void ReleaseProjectile(BaseProjectile projectile);    
    
    #endregion
    
    #region Private methods

    protected abstract void Shoot(Monster monster);

    protected abstract bool CheckDistance(Monster monster);

    protected abstract BaseProjectile GetProjectilePrefab();

    #endregion
}