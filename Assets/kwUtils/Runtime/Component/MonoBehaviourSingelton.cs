using System;
using UnityEngine;

namespace KWUtils
{
    public abstract class SingletonMonoBehaviour<T> : MonoBehaviour
    where T : Component
    {
        protected static T instance;
        private static readonly object lockInstance = new object();

        public static T Instance
        {
            /*
            get
            {
                if (instance != null) return instance;
                T[] objects = FindObjectsOfType(typeof(T)) as T[];
                if (objects?.Length > 0) instance = objects[0];
                if (objects?.Length > 1) UnityEngine.Debug.LogError($"there is more than one {typeof(T).Name} in the scene");
                if (instance == null)
                {
                    instance = new GameObject(typeof(T).Name).AddComponent<T>();
                    DontDestroyOnLoad(instance);
                }
                return instance;
            }
            */
            get
            {
                if (instance != null) return instance;
                lock (lockInstance)
                {
                    if (instance != null) return instance;
                    
                    instance = new GameObject(typeof(T).Name).AddComponent<T>();
                    DontDestroyOnLoad(instance);
                }
                return instance;
            }
        }
    }

    public abstract class Singleton<T> : MonoBehaviour 
    where T : MonoBehaviour
    {
        public static T Instance { get; protected set; }

        protected virtual void Awake() //DONT FORGET base.Awake();
        {
            if (Instance != null && Instance != this as T)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this as T;
        }
    }
}