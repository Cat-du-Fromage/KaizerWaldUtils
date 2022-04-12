using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

using static Unity.Mathematics.math;

namespace KWUtils.KwTerrain
{
    //=====================================================================
    // JOB SYSTEM
    //=====================================================================
    
    //VERTICES POSITION
    //==================================================================================================================
    [BurstCompile(CompileSynchronously = true)]
    public struct JVerticesPosition : IJobFor
    {
        [ReadOnly] private int MapSizeX;
        [NativeDisableParallelForRestriction]
        [WriteOnly] private NativeArray<float3> Vertices;

        public JVerticesPosition(int mapSize, NativeArray<float3> vertices)
        {
            MapSizeX = mapSize;
            Vertices = vertices;
        }
        
        public void Execute(int index)
        {
            int z = index / MapSizeX;
            int x = index - (z * MapSizeX);
            
            float3 pointPosition = float3(x, 0, z);
            Vertices[index] = pointPosition;
        }
    }
    
    //UVS
    //==================================================================================================================
    [BurstCompile(CompileSynchronously = true)]
    public struct JUvs : IJobFor
    {
        [ReadOnly] private int MapSizeX;
        
        [NativeDisableParallelForRestriction, WriteOnly]
        private NativeArray<float2> Uvs;

        public JUvs(int mapSizeX, NativeArray<float2> uvs)
        {
            MapSizeX = mapSizeX;
            Uvs = uvs;
        }
        
        public void Execute(int index)
        {
            float z = floor((float)index / MapSizeX);
            float x = index - (z * MapSizeX);
            Uvs[index] = float2(x / MapSizeX, z / MapSizeX);
        }
    }
    
    //TRIANGLES
    //==================================================================================================================
    [BurstCompile(CompileSynchronously = true)]
    public struct JTriangles : IJobFor
    {
        [ReadOnly] private int MapSizeX;
        [NativeDisableParallelForRestriction]
        [WriteOnly] private NativeArray<int> Triangles;

        public JTriangles(int mapSizeX, NativeArray<int> triangles)
        {
            MapSizeX = mapSizeX;
            Triangles = triangles;
        }
        
        public void Execute(int index)
        {
            int mapPoints = MapSizeX - 1;
            
            int z = index / mapPoints;
            int x = index - (z * mapPoints);
            int baseTriIndex = index * 6;
            
            int vertexIndex = index + select(z,1 + z, x > mapPoints);
            int4 trianglesVertex = int4(vertexIndex, vertexIndex + MapSizeX + 1, vertexIndex + MapSizeX, vertexIndex + 1);

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