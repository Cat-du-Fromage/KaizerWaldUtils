using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

namespace KWUtils
{
    [Serializable]
    public struct ChunkComponent
    {
        //private int chunkIndex;
        //private int2 chunkCoord;
        // Il faudra générer les portes à l'externe, les portes ici seront des références
        // Les changements sur l'array de référence impactera ainsi les chunks concernés
        public Memory<GateCluster> TopGates;
        public Memory<GateCluster> RightGates;
        public Memory<GateCluster> BottomGates;
        public Memory<GateCluster> LeftGates;

        public bool IsSideOpen(int side)
        {
            return side switch
            {
                0 => CheckGate(TopGates),
                1 => CheckGate(RightGates),
                2 => CheckGate(BottomGates),
                3 => CheckGate(LeftGates),
                _ => false
            };
        }

        private bool CheckGate(Memory<GateCluster> gatesAtSide)
        {
            Span<GateCluster> temp = gatesAtSide.Span;
            for (int i = 0; i < temp.Length; i++)
            {
                if (!temp[i].IsClosed) return true;
            }
            return false;
        }
    }
}
