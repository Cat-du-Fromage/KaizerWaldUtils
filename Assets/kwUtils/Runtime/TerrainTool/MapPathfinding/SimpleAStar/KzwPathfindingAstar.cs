using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KWUtils
{
    public class KzwPathfindingAstar : MonoBehaviour
    {
        private bool hasTerrain;
        private KzwTerrainGenerator terrain;

        private void Awake()
        {
            hasTerrain = TryGetComponent(out terrain);
            if(!hasTerrain) UnityEngine.Debug.LogError("No Terrain Found");
        }
        
        private void Update()
        {
            if (!hasTerrain) return;
            
        }
    }
}
