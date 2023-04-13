using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

namespace KWUtils
{
    public enum ClusterSide : int
    {
        Top    = 0,
        Right  = 1,
        Bottom = 2,
        Left   = 3,
    }
    public struct Gate
    {
        public int index1;
        public int index2;
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
            GetClustersAt(ClusterSide.Top, chunkQuadPerLine, ref topGates);
            GetClustersAt(ClusterSide.Right, chunkQuadPerLine, ref rightGates);
            GetClustersAt(ClusterSide.Bottom, chunkQuadPerLine, ref bottomGates);
            GetClustersAt(ClusterSide.Left, chunkQuadPerLine, ref leftGates);
        }
        
        private void GetClustersAt(ClusterSide side, int chunkQuadPerLine, ref List<GateWay> clusterSide)
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
                        ClusterSide.Top    => KWmath.Sq(chunkQuadPerLine) - chunkQuadPerLine + i,
                        ClusterSide.Right  => (chunkQuadPerLine - 1) + (chunkQuadPerLine * i),
                        ClusterSide.Bottom => i,
                        ClusterSide.Left   => chunkQuadPerLine * i,
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
