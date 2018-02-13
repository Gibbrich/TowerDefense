using Gamelogic.Extensions;
using UnityEngine;

namespace Game
{
    public static class Utils
    {
        public static T GetNewObjectSilently<T>(this Pool<T> pool, int delta) where T : class
        {
            if (!pool.IsObjectAvailable)
            {
                pool.IncCapacity(delta);
            }

            return pool.GetNewObject();
        }
    }
}