using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
namespace KWUtils
{
    public partial class KzwHPAGenerator : MonoBehaviour
    {
        public bool DebugHPA = false;
        
        private void OnDrawGizmos()
        {
            if (!DebugHPA) return;
            if (TerrainGates == null) return;
            for (int i = 0; i < TerrainGates.Length; i++)
            {
                Gate gate = TerrainGates[i];
                bool isHorizontal = (gate.Index2 - gate.Index1 == 1);
                Gizmos.color = isHorizontal ? Color.green : Color.red;
                float radius = isHorizontal ? 0.3f : 0.4f;
                Gizmos.DrawWireSphere(terrain.Grid.GridArray[gate.Index1].Center, radius);
                Gizmos.DrawWireSphere(terrain.Grid.GridArray[gate.Index2].Center, radius);
            }

        }
    }
}
#endif