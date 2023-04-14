using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Serialization;
using static KWUtils.KWmath;
using static KWUtils.KWGrid;
using static KWUtils.ChunkBuilderUtils;

using static Unity.Jobs.LowLevel.Unsafe.JobsUtility;
using static Unity.Mathematics.math;

using static UnityEngine.Mesh;
using static Unity.Collections.Allocator;
using static Unity.Collections.NativeArrayOptions;

using Mesh = UnityEngine.Mesh;
using int2 = Unity.Mathematics.int2;
#if UNITY_EDITOR
namespace KWUtils
{
    public partial class KzwTerrainGenerator : MonoBehaviour
    {
        public bool DebugMode = false;
        public bool GridCellVerticesDebug = false;
        public GridCell[] cellsDebug;
        
// =====================================================================================================================
// EDITOR
// =====================================================================================================================
        private void OnValidate()
        {
            Settings = new TerrainSettings(NumQuadPerLine, NumChunkXY, NoiseSettings);
            NumQuadPerLine = Settings.NumQuadPerLine;
            NumChunkXY = Settings.NumChunkAxis;
        }
        
        private void OnDrawGizmos()
        {
            if (!DebugMode) return;
            GridCellDebug();

        }

        private void GridCellDebug()
        {
            if (!GridCellVerticesDebug) return;
            if (Grid?.GridArray == null) return;
            if (cellsDebug == null)
            {
                cellsDebug = Grid.GridArray;
            }

            for (int i = 0; i < Grid.GridArray.Length; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    Vector3 offset = GetOffsetDebug(j);
                    Vector3 pos = Grid.GridArray[i].Vertices.ElementAt(j);
                    Handles.Label(pos + offset, $"{i}");
                }
            }
            
            Vector3 GetOffsetDebug(int index)
            {
                int2 coord = GetXY2(index, 2);
                float yOffset = select(-0.25f,0.25f,coord.y == 0);
                float xOffset = select(-0.25f,0.15f,coord.x == 0);
                return new Vector3(xOffset, 0, yOffset);
            }
        }
    }
}
#endif