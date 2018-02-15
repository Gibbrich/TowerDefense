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
            if (translation.magnitude > Speed)
            {
                translation = translation.normalized * Speed;
            }
            transform.Translate(translation);
        }
        
        #endregion
    }
}