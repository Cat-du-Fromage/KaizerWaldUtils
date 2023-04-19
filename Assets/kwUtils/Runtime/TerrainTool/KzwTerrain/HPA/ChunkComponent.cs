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
        public ArraySegment<Gate> TopGates;
        public ArraySegment<Gate> RightGates;
        public ArraySegment<Gate> BottomGates;
        public ArraySegment<Gate> LeftGates;

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

        private bool CheckGate(ArraySegment<Gate> gatesAtSide)
        {
            for (int i = 0; i < gatesAtSide.Count; i++)
            {
                if (!gatesAtSide[i].IsClosed) return true;
            }
            return false;
        }
    }
}
