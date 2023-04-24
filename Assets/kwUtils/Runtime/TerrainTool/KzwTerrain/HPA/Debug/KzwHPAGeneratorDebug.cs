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
        
        [Header("By Chunk")]
        public bool Enable_ChunkComponent_Debug = false;
        public bool Enable_SideState_Debug = false;
        
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
            //if (TerrainGates == null) return;
            if (GatesGridSystem == null) return;

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
                    for (int i = 0; i < GatesGridSystem.TerrainGates.Length; i++)
                    {
                        DrawGate(GatesGridSystem.TerrainGates[i]);
                    }
                    return true;
                }
                return false;
            }

            void DrawGateByRangeIndex()
            {
                int numRangeGateIndex = math.csum(GatesGridSystem.NumSpaceHV);
                int maxRangeGateIndex = numRangeGateIndex - 1;
                
                if (Clavier.numpadPlusKey.wasReleasedThisFrame)
                    currentRangeGateIndex = currentRangeGateIndex >= maxRangeGateIndex ? 0 : currentRangeGateIndex+1;
                if (Clavier.numpadMinusKey.wasReleasedThisFrame)
                    currentRangeGateIndex = currentRangeGateIndex == 0 ? numRangeGateIndex-1 : currentRangeGateIndex-1;
                for (int i = 0; i < GatesGridSystem.GateClusters[currentRangeGateIndex].Length; i++)
                    DrawGate(GatesGridSystem.GateClusters[currentRangeGateIndex].GateSlice.Span[i]);
            }
        }
        
        /*
         * DRAW CHUNKS COMPONENT: Draw gates connect
         */
        private void ChunkComponentDebug()
        {
            if (!Enable_ChunkComponent_Debug) return;
            //if (gatesGrid.ChunkComponents == null) return;
            int chunkLastIndex = GatesGridSystem.ChunkComponents.Length - 1;
            if (Keyboard.current.numpadPlusKey.wasReleasedThisFrame)
            {
                currentChunkIndex = currentChunkIndex >= chunkLastIndex ? 0 : currentChunkIndex + 1;
            }
            if (Keyboard.current.numpadMinusKey.wasReleasedThisFrame)
            {
                currentChunkIndex = currentChunkIndex == 0 ? chunkLastIndex : currentChunkIndex - 1;
            }
            
            ChunkComponent component = GatesGridSystem.ChunkComponents[currentChunkIndex];
            if (!component.TopGates.IsEmpty)    DrawRangeGate(component.TopGates.Span[0].GateSlice);
            if (!component.RightGates.IsEmpty)  DrawRangeGate(component.RightGates.Span[0].GateSlice);
            if (!component.BottomGates.IsEmpty) DrawRangeGate(component.BottomGates.Span[0].GateSlice);
            if (!component.LeftGates.IsEmpty)   DrawRangeGate(component.LeftGates.Span[0].GateSlice);


            GateClusterDebug();
        }

        private void GateClusterDebug()
        {
            if (!Enable_SideState_Debug) return;
            ChunkComponent component = GatesGridSystem.ChunkComponents[currentChunkIndex];
            if (!component.TopGates.IsEmpty)
            {
                if (Keyboard.current.upArrowKey.wasReleasedThisFrame) component.TopGates.Span[0].Toggle();
                DrawClosedSide(component.TopGates.Span[0]);
            }

            if (!component.RightGates.IsEmpty)
            {
                if (Keyboard.current.rightArrowKey.wasReleasedThisFrame) component.RightGates.Span[0].Toggle();
                DrawClosedSide(component.RightGates.Span[0]);
            }

            if (!component.BottomGates.IsEmpty)
            {
                if (Keyboard.current.downArrowKey.wasReleasedThisFrame) component.BottomGates.Span[0].Toggle();
                DrawClosedSide(component.BottomGates.Span[0]);
            }

            if (!component.LeftGates.IsEmpty)
            {
                if (Keyboard.current.leftArrowKey.wasReleasedThisFrame) component.LeftGates.Span[0].Toggle();
                DrawClosedSide(component.LeftGates.Span[0]);
            }
        }

        // UTILITIES
        // =============================================================================================================
        private void DrawRangeGate(Gate[] gates) { foreach (Gate gate in gates) DrawGate(gate); }
        private void DrawRangeGate(ArraySegment<Gate> gates) { foreach (Gate gate in gates) DrawGate(gate); }
        private void DrawRangeGate(Memory<Gate> gates) { foreach (Gate gate in gates.Span) DrawGate(gate); }
        private void DrawGate(Gate gate)
        {
            bool isHorizontal = (gate.Index2 - gate.Index1 == 1);
            Gizmos.color = isHorizontal ? Color.magenta : Color.cyan;
            float radius = isHorizontal ? 0.3f : 0.4f;
            Gizmos.DrawWireSphere(terrain.Grid.GridArray[gate.Index1].Center, radius);
            Gizmos.DrawWireSphere(terrain.Grid.GridArray[gate.Index2].Center, radius);
        }
        
        private void DrawClosedSide(GateCluster cluster)
        {
            
            Vector3 offsetRight = Vector3.right * 0.5f;
            Vector3 offsetForward = Vector3.forward * 0.5f;
            float offsetLengthMul = (terrain.Grid.GridData.NumCellInChunkX - 1) * 0.5f;

            bool isHorizontal = cluster.Orientation == GateOrientation.Horizontal;
            Vector3 position = terrain.Grid.GridArray[cluster[0].Index1].Center;
            
            position += isHorizontal ? offsetRight : offsetForward;
            position += isHorizontal ? Vector3.forward * offsetLengthMul : Vector3.right * offsetLengthMul;

            Vector3 size = isHorizontal
                ? new Vector3(1, 2, terrain.Grid.GridData.NumCellInChunkX)
                : new Vector3(terrain.Grid.GridData.NumCellInChunkX, 2, 1);
            
            Gizmos.color = cluster.IsClosed ? Color.red : Color.green;
            Gizmos.DrawWireCube(position, size);
        }
    }
}
#endif