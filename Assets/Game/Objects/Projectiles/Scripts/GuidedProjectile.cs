using Gamelogic.Extensions;
using UnityEngine;

namespace Game
{
    public class GuidedProjectile : BaseProjectile
    {
        #region Private fields
        
        private GameObject target;
        
        #endregion
        
        #region Unity callbacks
        
        void Update()
        {
            if (target == null || !target.activeSelf)
            {
                tower.ReleaseProjectile(this);
                return;
            }

            var translation = target.transform.position - transform.position;
            if (translation.magnitude > Speed)
            {
                translation = translation.normalized * Speed;
            }
            transform.Translate(translation);
        }
        
        #endregion
        
        #region Public methods

        public void SetTarget(GameObject target)
        {
            this.target = target;
        }
        
        #endregion
    }
}