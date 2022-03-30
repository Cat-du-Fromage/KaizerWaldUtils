using System.Collections.Generic;
using UnityEngine;

namespace KWUtils
{
    public interface IUpdateable
    {
        void NormalUpdate(); //I would call it update, but this way it doesn't collide with the unity message
    }
 
    public class UpdateManager : SingletonMonoBehaviour<UpdateManager>
    {

        void Awake()
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }

        //FIELDS
        //==============================================================================================================
 
        private static readonly HashSet<IUpdateable> table = new HashSet<IUpdateable>();
        
 
        //METHODS
        //==============================================================================================================
 
        public static void Register(IUpdateable obj)
        {
            if(obj == null) throw new System.ArgumentNullException();
            table.Add(obj);
        }
 
        public static void Unregister(IUpdateable obj)
        {
            if(obj == null) throw new System.ArgumentNullException();
 
            table.Remove(obj);
        }
 
        void Update()
        {
            using HashSet<IUpdateable>.Enumerator e = table.GetEnumerator(); //avoid gc by calling GetEnumerator and iterating manually
            while(e.MoveNext())
            {
                e.Current?.NormalUpdate();
            }
        }
    }
 /*
    public class SomeExample : MonoBehaviour, IUpdateable
    {
 
        void OnEnable()
        {
            UpdateManager.Register(this);
        }
 
        void OnDisable()
        {
            UpdateManager.Unregister(this);
        }
 
        //I implement explicitly for readability, you could do it implicitly
        void IUpdateable.Tick()
        {
            //perform update code
        }
 
    }
    */
}