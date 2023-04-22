using System;
using UnityEngine.Serialization;
using static Unity.Mathematics.math;

namespace KWUtils
{
    [Serializable]
    public struct Gate
    {
        public bool IsClosed; // facilité pour math.any qui ne reconnaît que le cas true
        public int Index1;
        public int Index2;

        public Gate(int index1, int index2)
        {
            IsClosed = false;
            Index1 = index1;
            Index2 = index2;
        }
        
        public int this[int index] => index == 0 ? Index1 : Index2;
    }
}