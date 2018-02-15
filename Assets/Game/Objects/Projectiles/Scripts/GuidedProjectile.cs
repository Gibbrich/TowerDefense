using Gamelogic.Extensions;
using UnityEngine;

namespace Game
{
    public class GuidedProjectile : BaseProjectile
    {
        public GameObject m_target;

        #region Unity callbacks
        
        void Update()
        {
            if (m_target == null)
            {
                Destroy(gameObject);
                return;
            }

            var translation = m_target.transform.position - transform.position;
            if (translation.magnitude > speed)
            {
                translation = translation.normalized * speed;
            }
            transform.Translate(translation);
        }
        
        #endregion
    }
}