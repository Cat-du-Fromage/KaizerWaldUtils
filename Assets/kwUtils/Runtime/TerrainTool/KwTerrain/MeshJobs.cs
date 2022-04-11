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
        [ReadOnly] private int MapSize;
        [ReadOnly] private int PointPerAxis;
        [ReadOnly] private float Spacing;
        [NativeDisableParallelForRestriction]
        [WriteOnly] private NativeArray<float3> Vertices;

        public JVerticesPosition(in MapSettings mapSettings, NativeArray<float3> vertices)
        {
            MapSize = mapSettings.mapSize;
            PointPerAxis = mapSettings.mapPointPerAxis;
            Spacing = mapSettings.pointSpacing;
            Vertices = vertices;
        }
        
        public void Execute(int index)
        {
            int z = index / PointPerAxis;
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
        [ReadOnly] private int MapPointPerAxis;
        
        [NativeDisableParallelForRestriction, WriteOnly]
        private NativeArray<float2> Uvs;

        public JUvs(in MapSettings mapSettings, NativeArray<float2> uvs)
        {
            MapPointPerAxis = mapSettings.mapPointPerAxis;
            Uvs = uvs;
        }
        
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
        [ReadOnly] private int MapPointPerAxis;
        [NativeDisableParallelForRestriction]
        [WriteOnly] private NativeArray<int> Triangles;

        public JTriangles(in MapSettings mapSettings, NativeArray<int> triangles)
        {
            MapPointPerAxis = mapSettings.mapPointPerAxis;
            Triangles = triangles;
        }
        
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