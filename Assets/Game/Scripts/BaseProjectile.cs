using System.Collections;
using System.Collections.Generic;
using Game;
using UnityEngine;

public abstract class BaseProjectile : MonoBehaviour
{
    #region Editor tweakable fields
    
    [SerializeField] protected float speed = 0.2f;
    [SerializeField] protected int damage = 10;
    
    #endregion
    
    #region Properties
    
    public int Damage
    {
        get { return damage; }
    }    
    
    #endregion
    
    #region Unity callbacks

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<Monster>())
        {
            /* todo    - implement
             * @author - Артур
             * @date   - 13.02.2018
             * @time   - 21:07
            */            
        }
    }

    #endregion
}