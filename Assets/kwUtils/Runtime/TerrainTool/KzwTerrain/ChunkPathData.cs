using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

namespace KWUtils
{
    public enum HPASide : int
    {
        Top    = 0,
        Right  = 1,
        Bottom = 2,
        Left   = 3,
    }
    
    public struct Gate
    {
        //public bool IsClosed;
        public readonly int Index1;
        public readonly int Index2;

        public Gate(int index1, int index2)
        {
            Index1 = index1;
            Index2 = index2;
        }
        
        public int this[int index] => index == 0 ? Index1 : Index2;
    }
    public struct GateWay
    {
        public List<int> Indices;

        public GateWay(int size)
        {
            Indices = new List<int>(size);
        }
    }
    public class ChunkPathData : MonoBehaviour
    {
        //A METTRE DANS GRID
        List<KeyValuePair<int, Gate>> test;

        private int chunkIndex;
        private int2 coordInGrid;
        private bool4 openSides; // distingue si un côté est bloqué

        private List<bool> obstacles; // Distingue si les cellules sont bloquée ou non
        
        private List<GateWay> topGates;
        private List<GateWay> rightGates;
        private List<GateWay> bottomGates;
        private List<GateWay> leftGates;

        private void InitializeGates()
        {
            
        }
//======================================================================================================================
        // On repère les discontinuités sur les côtés
        // On utilisera ainsi le cluster pour les futur test de chemin
        private void RegisterGateCluster(int chunkQuadPerLine)
        {
            GetClustersAt(HPASide.Top, chunkQuadPerLine, ref topGates);
            GetClustersAt(HPASide.Right, chunkQuadPerLine, ref rightGates);
            GetClustersAt(HPASide.Bottom, chunkQuadPerLine, ref bottomGates);
            GetClustersAt(HPASide.Left, chunkQuadPerLine, ref leftGates);
        }
        
        private void GetClustersAt(HPASide side, int chunkQuadPerLine, ref List<GateWay> clusterSide)
        {
            if (!openSides[(int)side]) return;
            int clusterIndex = 0;
            clusterSide = new List<GateWay>(2) { new GateWay(chunkQuadPerLine) };
            for (int i = 0; i < chunkQuadPerLine; i++) // how do we get ChunkSize ?
            {
                if (!obstacles[i])
                {
                    int index = side switch
                    {
                        HPASide.Top    => KWmath.Sq(chunkQuadPerLine) - chunkQuadPerLine + i,
                        HPASide.Right  => (chunkQuadPerLine - 1) + (chunkQuadPerLine * i),
                        HPASide.Bottom => i,
                        HPASide.Left   => chunkQuadPerLine * i,
                        _ => throw new ArgumentOutOfRangeException(nameof(side), side, null)
                    };
                    clusterSide[clusterIndex].Indices.Add(index);
                }
                else
                {
                    clusterSide[clusterIndex].Indices.TrimExcess();
                    if (i == chunkQuadPerLine - 1) return;
                    clusterSide.Add(new GateWay(chunkQuadPerLine-i));
                    clusterIndex++;
                }
            }
            clusterSide[clusterIndex].Indices.TrimExcess();
            clusterSide.TrimExcess();
        }
//======================================================================================================================

        
    }
}
