using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;

using static Unity.Mathematics.math;

namespace KWUtils
{
    public class MapDatas : MonoBehaviour
    {
        public int2 MapSize;

        public void CheckMapSize()
        {
            MapSize = ceilpow2(MapSize);
        }
    }
}
