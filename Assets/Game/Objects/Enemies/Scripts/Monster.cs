using System;
using UnityEngine;
using Zenject;

namespace Game
{
    public class Monster : MonoBehaviour
    {
        const float REACH_DISTANCE = 0.3f;
    
        #region Editor tweakable fields
    
        [SerializeField] private float speed = 0.1f;
        [SerializeField] private int maxHealth = 30;
    
        #endregion
    
        #region Fields
        
        public event Action<Monster> Death = monster => { }; 
    
        private Spawner spawner;
        private Vector3 target;
        private int currentHealth;

        // used for speed calculation
        private Vector3 updatePosition;
        private Vector3 lateUpdatePosition;

        #endregion

        #region Properties
    
        public int CurrentHealth
        {
            get { return currentHealth; }
            private set { currentHealth = value; }
        }    
    
        #endregion
        
        #region Unity callbacks
        
        private void Start()
        {
            Refresh();
        }

        private void Update()
        {
            updatePosition = transform.position;
            
            if (Vector3.Distance(transform.position, target) <= REACH_DISTANCE)
            {
                spawner.Release(this);
                return;
            }

            var translation = target - transform.position;
            if (translation.magnitude > speed)
            {
                translation = translation.normalized * speed;
            }
            
            transform.Translate(translation);
        }

        private void LateUpdate()
        {
            lateUpdatePosition = transform.position;
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
                    Death(this);
                }                
            }
        }

        #endregion
        
        #region Public methods

        /// <summary>
        /// As we use Object pool pattern, we need refresh game object's state before reuse it.
        /// </summary>
        public void Refresh()
        {
            CurrentHealth = maxHealth;
            transform.position = spawner.transform.position;
        }

        public Vector3 GetSpeed()
        {
            return (lateUpdatePosition - updatePosition) / Time.deltaTime;
        }
        
        #endregion
        
        #region Private methods

        [Inject]
        private void Init([Inject(Id = "target")] Transform transformTarget,
                          [Inject] Spawner spawner)
        {
            target = transformTarget.position;
            this.spawner = spawner;
        }        
        
        #endregion
    }

    public class MonsterFactory: Factory<Monster>
    {
    }
}