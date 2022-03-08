using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KWUtils
{
    public static class TransformExtension
    {
        public static I GetInterfaceComponent<I>(this Transform transform) 
        where I : class
        {
            return transform.GetComponent(typeof(I)) as I;
        }
    }
}
