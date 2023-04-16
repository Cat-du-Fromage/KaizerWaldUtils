using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;

#if UNITY_EDITOR
namespace KWUtils
{
    public partial class KzwHPAGenerator : MonoBehaviour
    {
        private int currentChunkIndex = 1;
        
        private int currentRangeGateIndex = 0;
        private bool drawAllGate = false;
        
        public bool DebugHPA = false;
        public bool Enable_TerrainGates_Debug = false;
        public bool Enable_ChunkComponent_Debug = false;

        public Keyboard Clavier => Keyboard.current;
        private void OnDrawGizmos()
        {
            if(!Application.isPlaying) return;
            if (!DebugHPA) return;
            TerrainGatesDebug();
            ChunkComponentDebug();
        }

        private void TerrainGatesDebug()
        {
            if (!Enable_TerrainGates_Debug) return;
            if (TerrainGates == null) return;

            if (DrawAllGateOnly()) return;
            DrawGateByRangeIndex();
            
            // ---------------------------------------------------------------------------------------------------------
            // Internal Methods
            // ---------------------------------------------------------------------------------------------------------
            bool DrawAllGateOnly()
            {
                drawAllGate = Clavier.tabKey.wasReleasedThisFrame ? !drawAllGate : drawAllGate;
                if (drawAllGate)
                {
                    for (int i = 0; i < TerrainGates.Length; i++)
                    {
                        DrawGate(TerrainGates[i]);
                    }
                    return true;
                }
                return false;
            }

            void DrawGateByRangeIndex()
            {
                //int2 numChunkAxis = terrain.Settings.NumChunkAxis;
                //int2 numSpaceHV = new (math.max(0,numChunkAxis.x-1) * numChunkAxis.y, numChunkAxis.x * math.max(0, numChunkAxis.y-1));
                int numRangeGateIndex = math.csum(NumSpaceHV);
                int maxRangeGateIndex = numRangeGateIndex - 1;
                
                if (Clavier.numpadPlusKey.wasReleasedThisFrame)
                    currentRangeGateIndex = currentRangeGateIndex >= maxRangeGateIndex ? 0 : currentRangeGateIndex+1;
                if (Clavier.numpadMinusKey.wasReleasedThisFrame)
                    currentRangeGateIndex = currentRangeGateIndex == 0 ? numRangeGateIndex-1 : currentRangeGateIndex-1;
                for (int i = 0; i < GroupedGates[currentRangeGateIndex].Count; i++)
                    DrawGate(GroupedGates[currentRangeGateIndex][i]);
            }
        }
        
        private void ChunkComponentDebug()
        {
            if (!Enable_ChunkComponent_Debug) return;
            if (ChunkComponents == null) return;
            int chunkLastIndex = ChunkComponents.Length - 1;
            if (Keyboard.current.numpadPlusKey.wasReleasedThisFrame)
            {
                currentChunkIndex = currentChunkIndex >= chunkLastIndex ? 0 : currentChunkIndex + 1;
            }
            if (Keyboard.current.numpadMinusKey.wasReleasedThisFrame)
            {
                currentChunkIndex = currentChunkIndex == 0 ? chunkLastIndex : currentChunkIndex - 1;
            }
            
            ChunkComponent component = ChunkComponents[currentChunkIndex];
            if (!component.TopGates.IsNullOrEmpty())    DrawRangeGate(component.TopGates);
            if (!component.RightGates.IsNullOrEmpty())  DrawRangeGate(component.RightGates);
            if (!component.BottomGates.IsNullOrEmpty()) DrawRangeGate(component.BottomGates);
            if (!component.LeftGates.IsNullOrEmpty())   DrawRangeGate(component.LeftGates);
            
        }

        // UTILITIES
        private void DrawRangeGate(Gate[] gates) { foreach (Gate gate in gates) DrawGate(gate); }
        private void DrawRangeGate(ArraySegment<Gate> gates) { foreach (Gate gate in gates) DrawGate(gate); }
        
        private void DrawGate(Gate gate)
        {
            bool isHorizontal = (gate.Index2 - gate.Index1 == 1);
            Gizmos.color = isHorizontal ? Color.green : Color.red;
            float radius = isHorizontal ? 0.3f : 0.4f;
            Gizmos.DrawWireSphere(terrain.Grid.GridArray[gate.Index1].Center, radius);
            Gizmos.DrawWireSphere(terrain.Grid.GridArray[gate.Index2].Center, radius);
        }
    }
}
#endif