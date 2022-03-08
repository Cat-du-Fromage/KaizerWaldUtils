using UnityEngine;

namespace KWUtils
{
    public class Singleton : MonoBehaviour
    {
        public static Singleton Instance { get; private set; }
        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this);
                return;
            }
            Instance = this;
        }
    }
}