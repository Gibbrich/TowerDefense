﻿using UnityEngine;
using System.Collections;
using Game;

public class CannonProjectile : BaseProjectile
{
    #region Unity callbacks
    
    void Update()
    {
        var translation = transform.forward * Speed;
        transform.Translate(translation);
    }
    
    #endregion
    
    #region Public methods
    
        
    
    #endregion
}