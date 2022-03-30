using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

using static Unity.Mathematics.math;
using static Unity.Mathematics.float3;
using static KWUtils.KWmath;
using static KWUtils.NativeCollectionExt;

namespace KWUtils.KWTerrain
{
    //=====================================================================
    // JOB SYSTEM
    //=====================================================================
    
    //VERTICES POSITION
    //==================================================================================================================
    [BurstCompile(CompileSynchronously = true)]
    public struct JVerticesPosition : IJobFor
    {
        [ReadOnly] public int MapSize;
        [ReadOnly] public int PointPerAxis;
        [ReadOnly] public float Spacing;
        
        [NativeDisableParallelForRestriction] 
        [WriteOnly] public NativeArray<float3> Vertices;

        public void Execute(int index)
        {
            int z = (int)floor(index / (float)PointPerAxis);
            int x = index - (z * PointPerAxis);
            
            float3 pointPosition = float3(x, 0, z) * float3(Spacing) + float3(MapSize*-0.5f,0,MapSize*-0.5f);
            Vertices[index] = pointPosition;
        }
    }
    
    //UVS
    //==================================================================================================================
    [BurstCompile(CompileSynchronously = true)]
    public struct JUvs : IJobFor
    {
        [ReadOnly] public int MapPointPerAxis;
        [NativeDisableParallelForRestriction] 
        [WriteOnly] public NativeArray<float2> Uvs;

        
        public void Execute(int index)
        {
            float z = floor((float)index / MapPointPerAxis);
            float x = index - (z * MapPointPerAxis);
            Uvs[index] = float2(x / MapPointPerAxis, z / MapPointPerAxis);
        }
    }
    
    //TRIANGLES
    //==================================================================================================================
    [BurstCompile(CompileSynchronously = true)]
    public struct JTriangles : IJobFor
    {
        [ReadOnly] public int MapPointPerAxis;
        [NativeDisableParallelForRestriction]
        [WriteOnly] public NativeArray<int> Triangles;

        
        public void Execute(int index)
        {
            int mapPoints = MapPointPerAxis - 1;
            
            int z = (int)floor((float)index / mapPoints);
            int x = index - (z * mapPoints);
            int baseTriIndex = index * 6;
            
            int vertexIndex = index + select(z,1 + z, x > mapPoints);
            int4 trianglesVertex = int4(vertexIndex, vertexIndex + MapPointPerAxis + 1, vertexIndex + MapPointPerAxis, vertexIndex + 1);

            Triangles[baseTriIndex] = trianglesVertex.z;
            Triangles[baseTriIndex + 1] = trianglesVertex.y;
            Triangles[baseTriIndex + 2] = trianglesVertex.x;
            baseTriIndex += 3;
            Triangles[baseTriIndex] = trianglesVertex.w;
            Triangles[baseTriIndex + 1] = trianglesVertex.x;
            Triangles[baseTriIndex + 2] = trianglesVertex.y;
        }
    }
}
