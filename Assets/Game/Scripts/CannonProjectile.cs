using UnityEngine;
using System.Collections;
using Game;

public class CannonProjectile : BaseProjectile
{
    void Update()
    {
        var translation = transform.forward * speed;
        transform.Translate(translation);
    }

    void OnTriggerEnter(Collider other)
    {
        var monster = other.gameObject.GetComponent<Monster>();
        if (monster == null)
            return;
    }
}