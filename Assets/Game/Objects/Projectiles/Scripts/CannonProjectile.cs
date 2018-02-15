using UnityEngine;
using System.Collections;
using Game;

public class CannonProjectile : BaseProjectile
{
    #region Private fields

    private Vector3 direction;
    
    #endregion
    
    #region Unity callbacks

    void Update()
    {
        var translation = direction * Speed;
        transform.Translate(translation);
    }
    
    #endregion
    
    #region Public methods

    public void SetDirection(Vector3 direction)
    {
        this.direction = direction;
    }
    
    #endregion
}