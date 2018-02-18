using System.Collections;
using System.Collections.Generic;
using Game;
using Gamelogic.Extensions;
using UnityEngine;

public abstract class BaseProjectile : MonoBehaviour
{
    #region Editor tweakable fields
    
    [SerializeField] protected float speed = 0.2f;
    [SerializeField] protected int damage = 10;
    
    #endregion

    protected BaseTower tower;
    
    #region Properties
    
    public int Damage
    {
        get { return damage; }
    }

    public float Speed
    {
        get { return speed; }
    }

    #endregion
    
    #region Unity callbacks

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<Monster>())
        {
            tower.ReleaseProjectile(this);
        }
    }

    #endregion
    
    #region Public methods

    public static T Create<T>(T prefab, Vector3 position, Transform parent, BaseTower tower) where T : BaseProjectile
    {
        T projectile = Instantiate(prefab, position, Quaternion.identity, parent);
        projectile.tower = tower;

        return projectile;
    }

    #endregion
    
    #region Private methods

    public virtual void Refresh()
    {
    }
    
    #endregion
}