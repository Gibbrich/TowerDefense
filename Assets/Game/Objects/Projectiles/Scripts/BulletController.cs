using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletController : MonoBehaviour
{
    public int speed = 50;
    private Transform bulletItem;

    // Use this for initialization
    void Start () {
        bulletItem = transform;
    }
	
    // Update is called once per frame
    void Update () {
        bulletItem.position += bulletItem.forward * speed * Time.deltaTime;
    }

    protected virtual void OnTriggerEnter(Collider col) {
        Destroy(gameObject);
    }
}