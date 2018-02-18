﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CannonProjectile : BaseProjectile
{
    #region Editor tweakable fields
    
    [SerializeField] private float selfDestroyCountDown = 5f;        
    
    #endregion
    
    #region Private fields

    private float time;
    
    #endregion
        
    #region Unity callbacks
    
    private void Update()
    {
        if (Time.time - time >= selfDestroyCountDown)
        {
            tower.ReleaseProjectile(this);
        }
        else
        {
            transform.position += transform.forward * speed * Time.deltaTime;
        }
    }
    
    #endregion
    
    #region Public methods

    public override void Refresh()
    {
        time = Time.time;
    }
    
    #endregion
}