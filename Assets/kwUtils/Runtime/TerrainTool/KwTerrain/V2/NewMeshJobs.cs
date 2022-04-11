using System.Collections;
using System.Collections.Generic;
using KWUtils.ProceduralMeshes;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace KWUtils.KwTerrain
{
    public struct JMeshJob : IJobFor
    {
        [ReadOnly] public int2 MapSize;
        
        [NativeDisableContainerSafetyRestriction]
        public NativeArray<float3> Stream0;
        
        [NativeDisableContainerSafetyRestriction]
        public NativeArray<float3> Stream1;
        
        [NativeDisableContainerSafetyRestriction]
        public NativeArray<float4> Stream2;

        [NativeDisableContainerSafetyRestriction]
        public NativeArray<float2> Stream3;
        
        [NativeDisableContainerSafetyRestriction]
        public NativeArray<TriangleUInt16> Triangles;
        
        public void Execute(int index)
        {
            int vi = 4 * index;
            int ti = 2 * index;
        }
        
        private void SetVertex (int index, in Vertex vertex) 
        {
            Stream0[index] = vertex.position;
            Stream1[index] = vertex.normal;
            Stream2[index] = vertex.tangent;
            Stream3[index] = vertex.texCoord0;
        }
        
        public void SetTriangle(int index, in int3 triangle)
        {
            Triangles[index] = triangle;
        }
    }
}
