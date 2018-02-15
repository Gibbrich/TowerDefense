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

    private BaseTower tower;
    
    #region Properties
    
    public int Damage
    {
        get { return damage; }
    }    
    
    #endregion
    
    #region Unity callbacks

    private void OnTriggerEnter(Collider other)
    {
        tower.ReleaseProjectile(this);
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
}