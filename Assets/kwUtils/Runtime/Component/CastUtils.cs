using UnityEngine;

namespace KWUtils
{
    public static class CastUtils
    {
        public static I AsInterface<I>(this Component component)
        where I : class
        {
            if (component is I component1)
            {
                return component1;
            }
            return null;
        }

        
    }
}