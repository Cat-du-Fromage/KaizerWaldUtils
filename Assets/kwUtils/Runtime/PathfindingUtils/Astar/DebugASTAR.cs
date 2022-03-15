#if UNITY_EDITOR

using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

using Debug = UnityEngine.Debug;

namespace TowerDefense
{
    /*
    public partial class AStarPathfinding
    {
        public bool DebugAStar;
        private void OnDrawGizmos()
        {
            if (!DebugAStar) return;

            GUIStyle style = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter
            };
            
            if (Grid != null)
            {
                Vector3 pos;
                Vector3 cellBounds = new Vector3(CellSize, 0.05f, CellSize);
                for (int i = 0; i < Grid.GetGridArray.Length / 4; i++)
                {
                    pos = Grid.GetCenterCellAt(i);
                    Gizmos.DrawWireCube(pos, cellBounds);
                    int index = Grid.GetGridArray[i].Coord.y * Grid.GetGridWidth + Grid.GetGridArray[i].Coord.x;
                    Handles.Label(pos, index.ToString(), style);
                }
            }
            
            if (path == null) return;
            if (path.Length != 0)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawSphere(Grid.GetCenterCellAt(startIndex), 1f);
                Gizmos.color = Color.red;
                Gizmos.DrawSphere(Grid.GetCenterCellAt(endIndex), 1f);
                Gizmos.color = Color.black;
                for (int i = 0; i < path.Length; i++)
                {
                    Handles.Label(Grid.GetCenterCellAt(path[i]), path[i].ToString(), style);
                    Gizmos.DrawWireSphere(Grid.GetCenterCellAt(path[i]), 1f);
                }
            }
        }
    }
    */
}
#endif