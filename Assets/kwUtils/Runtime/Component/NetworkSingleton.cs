using System.Collections;
using System.Collections.Generic;
//using Unity.Netcode;
using UnityEngine;
/*
namespace KaizerWaldCode.Utils
{
    public abstract class NetworkSingleton<T> : NetworkBehaviour
    where T : Component
    {
        private static T instance;

        public static T Instance
        {
            get
            {
                if (instance is not null) return instance;
                
                T[] objects = FindObjectsOfType(typeof(T)) as T[];
                if (objects?.Length > 0) instance = objects[0];
                if (objects?.Length > 1) Debug.LogError($"there is more than one {typeof(T).Name} in the scene");
                return instance ?? new GameObject(typeof(T).Name).AddComponent<T>();
            }
        }
    }
}
*/