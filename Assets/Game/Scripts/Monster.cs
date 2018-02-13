using UnityEngine;
using System.Collections;
using Zenject;

public class Monster : MonoBehaviour
{
    const float REACH_DISTANCE = 0.3f;
    
    #region Editor tweakable fields
    
    public GameObject m_moveTarget;
    public float m_speed = 0.1f;
    public int m_maxHP = 30;
    
    #endregion
    
    #region Fields
    
    [Inject] private Spawner spawner;
    private int m_hp;

    #endregion

    #region Properties
    
    public int Hp
    {
        get { return m_hp; }
        set { m_hp = value; }
    }    
    
    #endregion

    void Start()
    {
        Hp = m_maxHP;
    }

    void Update()
    {
        if (m_moveTarget == null)
        {
            return;
        }

        if (Vector3.Distance(transform.position, m_moveTarget.transform.position) <= REACH_DISTANCE)
        {
            Destroy(gameObject);
            return;
        }

        var translation = m_moveTarget.transform.position - transform.position;
        if (translation.magnitude > m_speed)
        {
            translation = translation.normalized * m_speed;
        }
        transform.Translate(translation);
    }
}