using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KWUtils
{
    public static class GameObjectExtension
    {
        public static I GetInterfaceComponent<I>(this GameObject gameObject) 
        where I : class
        {
            return gameObject.GetComponent(typeof(I)) as I;
        }
        
        public static I GetInterfaceComponent<I>(this Component component) 
        where I : class
        {
            return component.gameObject.GetComponent(typeof(I)) as I;
        }

        public static List<I> FindObjectsOfInterface<I>() 
        where I : class
        {
            MonoBehaviour[] monoBehaviours = Object.FindObjectsOfType<MonoBehaviour>();
            List<I> list = new List<I>();
 
            foreach(MonoBehaviour behaviour in monoBehaviours)
            {
                I component = behaviour.GetComponent(typeof(I)) as I;
                if (component is null) continue;
                list.Add(component);
            }
            return list;
        }

        public static T2 GetComponentInChildrenFrom<T1,T2>(this GameObject gameObject) 
        where T1 : Component
        where T2 : Component
        {
            return gameObject.GetComponentInChildren<T1>().gameObject.GetComponent<T2>();
        }
        
        public static T2 GetComponentWithTag<T1,T2>(this GameObject gameObject) 
        where T1 : Component
        where T2 : Component
        {
            return gameObject.GetComponentInChildren<T1>().gameObject.GetComponent<T2>();
        }
        
        public static T2 GetWithTagComponentInChildren<T1,T2>(this GameObject gameObject) 
        where T1 : Component
        where T2 : Component
        {
            return gameObject.GetComponentInChildren<T1>().gameObject.GetComponentInChildren<T2>();
        }

        public static T GetOrAddComponent<T>(this GameObject gameObject)
        where T : Component
        {
            return gameObject.TryGetComponent(out T component) ? component : gameObject.AddComponent<T>();
        }
    }
}
