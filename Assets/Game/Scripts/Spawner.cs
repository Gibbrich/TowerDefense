using UnityEngine;
using System.Collections;

public class Spawner : MonoBehaviour
{
    #region Editor tweakable fields
    
    [SerializeField] private float m_interval = 3;
    [SerializeField] private GameObject m_moveTarget;
    
    #endregion
    
    #region Fields
    
    private float m_lastSpawn = -1;
    
    #endregion

    #region Unity callbacks
    
    void Update()
    {
        if (Time.time > m_lastSpawn + m_interval)
        {
            var newMonster = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            var r = newMonster.AddComponent<Rigidbody>();
            r.useGravity = false;
            newMonster.transform.position = transform.position;
            var monsterBeh = newMonster.AddComponent<Monster>();
            monsterBeh.m_moveTarget = m_moveTarget;

            m_lastSpawn = Time.time;
        }
    }
    
    #endregion

}