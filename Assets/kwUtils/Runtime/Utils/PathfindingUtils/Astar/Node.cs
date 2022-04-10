using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

namespace KWUtils
{
    public readonly struct Node
    {
        public readonly int CameFromNodeIndex;
        
        public readonly int GCost; //Distance from Start Node
        public readonly int HCost; // distance from End Node
        public readonly int FCost;
        public readonly int2 Coord;

        public Node(int cameFromNodeIndex, int gCost, int hCost, in int2 coord)
        {
            CameFromNodeIndex = cameFromNodeIndex;
            GCost = gCost;
            HCost = hCost;
            FCost = GCost + HCost;
            Coord = coord;
        }

        public Node(in int2 coord)
        {
            CameFromNodeIndex = -1;
            GCost = int.MaxValue;
            HCost = default;
            FCost = GCost + HCost;
            Coord = coord;
        }
    }
}
