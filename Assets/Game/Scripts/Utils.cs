using Gamelogic.Extensions;
using UnityEngine;

namespace Game
{
    public static class Utils
    {
        public static T GetNewObjectSilently<T>(this Pool<T> pool, int increaseAmount = 10) where T : class
        {
            if (!pool.IsObjectAvailable)
            {
                pool.IncCapacity(increaseAmount);
            }

            return pool.GetNewObject();
        }
    }
}