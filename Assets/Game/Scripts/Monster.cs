using UnityEngine;
using Zenject;

namespace Game
{
    public class Monster : MonoBehaviour
    {
        const float REACH_DISTANCE = 0.3f;
    
        #region Editor tweakable fields
    
        public float m_speed = 0.1f;
        public int maxHealth = 30;
    
        #endregion
    
        #region Fields
    
        [Inject] private Spawner spawner;
        [Inject(Id = "target")] private Vector3 target;
        
        private int currentHealth;

        #endregion

        #region Properties
    
        public int CurrentHealth
        {
            get { return currentHealth; }
            set { currentHealth = value; }
        }    
    
        #endregion
        
        #region Unity callbacks
        
        void Start()
        {
            CurrentHealth = maxHealth;
        }

        void Update()
        {
            if (Vector3.Distance(transform.position, target) <= REACH_DISTANCE)
            {
                spawner.Release(this);
                return;
            }

            var translation = target - transform.position;
            if (translation.magnitude > m_speed)
            {
                translation = translation.normalized * m_speed;
            }
            transform.Translate(translation);
        }

        private void OnTriggerEnter(Collider other)
        {
            var projectile = other.GetComponent<BaseProjectile>();
            if (projectile)
            {
                currentHealth -= projectile.Damage;

                if (currentHealth <= 0)
                {
                    spawner.Release(this);
                }                
            }
        }

        #endregion
    }

    public class MonsterFactory: Factory<Monster>
    {
    }
}