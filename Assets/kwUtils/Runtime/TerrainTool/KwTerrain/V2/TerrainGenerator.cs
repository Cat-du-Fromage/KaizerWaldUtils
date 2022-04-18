using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

using static UnityEngine.Mesh;
using static Unity.Mathematics.math;
using static KWUtils.KWmath;
using static KWUtils.NativeCollectionExt;
using static Unity.Jobs.LowLevel.Unsafe.JobsUtility;
using int2 = Unity.Mathematics.int2;

namespace KWUtils.KwTerrain
{
    public readonly struct KwTerrainData
    {
        public readonly int ChunkSize;
        public readonly int2 TerrainNumQuadsXZ;
        public readonly int2 ChunkNumQuadsXZ;
        
        public readonly int2 TerrainNumVerticesXZ; //: (TerrainNumQuadsXZ + int2(1,1))
        public readonly int2 ChunkNumVerticesXZ; //: (ChunkNumQuadsXZ + int2(1,1))

        public KwTerrainData(in int2 numQuadsXZ, int chunkSIze)
        {
            ChunkSize = ceilpow2(chunkSIze);
            TerrainNumQuadsXZ = ceilpow2(numQuadsXZ);
            
            while (ChunkSize > cmin(TerrainNumQuadsXZ))
            {
                if (ChunkSize == 1) break;
                ChunkSize >>= 1;
            }
            
            ChunkNumQuadsXZ = TerrainNumQuadsXZ / ChunkSize;
            TerrainNumVerticesXZ = TerrainNumQuadsXZ + int2(1);
            ChunkNumVerticesXZ = ChunkNumQuadsXZ + int2(1);
        }
    }
    
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
    public class TerrainGenerator : MonoBehaviour
    {
        [SerializeField] private Material defaultChunkMaterial;
        
        //Data
        
        //private int2 TerrainNumQuadsXZ
        //private int2 ChunkNumQuadsXZ
        
        //private int2 TerrainNumVerticesXZ //: (TerrainNumQuadsXZ + int2(1,1))
        //private int2 ChunkNumVerticesXZ //: (ChunkNumQuadsXZ + int2(1,1))
        
        //CAREFULL Size NOT EQUAL Vertices : => need + 1
        [SerializeField] private int2 MapSize = int2(1);
        [SerializeField] private int ChunkSize = 1;

        private KwTerrainData kwTerrainData;
        
        private Mesh mesh;

        private Dictionary<int, GameObject> chunks;

        private void OnValidate()
        {
            CheckValues();
        }

        private void Awake()
        {
            kwTerrainData = new KwTerrainData(MapSize, ChunkSize);
            CheckValues();
            CreateChunks();
            
            GenerateBigMap();
            GetAndAssignMeshDatas();
        }

        private void CreateChunks()
        {
            int numChunk = cmul(MapSize / ChunkSize);
            chunks = new Dictionary<int, GameObject>(numChunk);
            
            for (int i = 0; i < numChunk; i++)
            {
                int2 PositionInWorld = i.GetXY2((MapSize / ChunkSize).x);
                chunks.TryAdd(i, new GameObject($"Chunk_{i}", typeof(TerrainChunkComponent)));

                float2 center = (PositionInWorld * ChunkSize) + (ChunkSize / 2);
                Mesh chunkMesh = new Mesh 
                {
                    name = $"ChunkMesh_{i}",
                    bounds = new Bounds(new Vector3(center.x,0,center.y), new Vector3(ChunkSize,0,ChunkSize)),
                };
                chunks[i].transform.localPosition = new Vector3(PositionInWorld.x*(ChunkSize), 0, PositionInWorld.y*(ChunkSize));
                chunks[i].GetComponent<MeshFilter>().mesh = chunkMesh;
                chunks[i].GetComponent<Renderer>().material = defaultChunkMaterial;
            }
        }
        
        // Check All value are Pow2
        private void CheckValues()
        {
            ChunkSize = ceilpow2(ChunkSize);
            MapSize = ceilpow2(MapSize);
            
            while (ChunkSize > cmin(MapSize))
            {
                if (ChunkSize == 1) break;
                ChunkSize >>= 1;
            }
        }
/*
        public void GetAndAssignMeshDatas()
        {
            int numVertices = cmul((MapSize/ChunkSize) * (ChunkSize));
            UnityEngine.Debug.Log(numVertices);
            
            //Vertices
            using NativeArray<float3> verticesTemp = AllocNtvAry<float3>(numVertices);
            JVerticesPosition vJob = new JVerticesPosition(MapSize.x, verticesTemp);
            JobHandle vJobHandle = vJob.ScheduleParallel(numVertices, JobWorkerCount - 1, default);

            using NativeArray<int> triangles = AllocNtvAry<int>(Sq(ChunkSize-1) * 6);
            JTriangles tJob = new JTriangles(ChunkSize, triangles);
            JobHandle tJobHandle = tJob.ScheduleParallel(Sq(ChunkSize), JobWorkerCount - 1, vJobHandle);
            
            using NativeArray<float2> uvs = AllocNtvAry<float2>(numVertices);
            JUvs uJob = new JUvs(MapSize.x, uvs);
            JobHandle uJobHandle = uJob.ScheduleParallel(numVertices, JobWorkerCount - 1, tJobHandle);
            //uJobHandle.Complete();

            //Ordered Array by Chunks
            GridData gridData = new GridData(MapSize, 1, ChunkSize);
            using NativeArray<float3> verticesOrderedTemp = AllocNtvAry<float3>(numVertices);
            JobHandle verticesOrderJobHandle = verticesOrderedTemp.OrderNativeArrayByChunk(verticesTemp, gridData, uJobHandle);
            
            using NativeArray<float2> uvsOrdered = AllocNtvAry<float2>(numVertices);
            JobHandle uvsOrderJobHandle = uvsOrdered.OrderNativeArrayByChunk<float2>(uvs, gridData, verticesOrderJobHandle);
            uvsOrderJobHandle.Complete();

            int numChunk = cmul(MapSize / ChunkSize);
            int numVerticesChunk = Sq(ChunkSize);
            UnityEngine.Debug.Log($"Vertices per chunk = {numVerticesChunk}");
            
            int[] tri = triangles.ToArray();
            for (int i = 0; i < numChunk; i++)
            {
                Mesh chunkMesh = chunks[i].GetComponent<MeshFilter>().mesh;
                
                chunkMesh.SetVertices(verticesOrderedTemp.GetSubArray(0, numVerticesChunk).Reinterpret<Vector3>());
                chunkMesh.SetTriangles(tri,0);
                chunkMesh.SetUVs(0,uvsOrdered.GetSubArray(i * numVerticesChunk, numVerticesChunk).Reinterpret<Vector2>());
                
                chunkMesh.RecalculateNormals();
                chunkMesh.RecalculateTangents();
            }
            
        }
*/
        public void GetAndAssignMeshDatas()
        {
            int numVertices = cmul(kwTerrainData.TerrainNumVerticesXZ);
            int totalChunkVertices = cmul(kwTerrainData.ChunkNumVerticesXZ);
            int totalChunkQuads = cmul(kwTerrainData.ChunkNumQuadsXZ);
            UnityEngine.Debug.Log(numVertices);
            
            UnityEngine.Debug.Log($"TotalNumQuadsXZ = {cmul(kwTerrainData.ChunkNumQuadsXZ)}");
            UnityEngine.Debug.Log($"TotalNumVerticesXZ = {cmul(kwTerrainData.ChunkNumVerticesXZ)}");
            UnityEngine.Debug.Log($"chunkQuad = {MapSize / ChunkSize}");
            
            //Vertices
            using NativeArray<float3> verticesTemp = AllocNtvAry<float3>(totalChunkVertices);
            JVerticesPosition2 vJob = new (kwTerrainData.ChunkNumVerticesXZ.x, verticesTemp);
            JobHandle vJobHandle = vJob.ScheduleParallel(totalChunkVertices, JobWorkerCount - 1, default);

            //Triangles : Use Quads!
            using NativeArray<int> triangles = AllocNtvAry<int>(totalChunkQuads * 6);
            JTriangles2 tJob = new (kwTerrainData.ChunkNumQuadsXZ.x, kwTerrainData.ChunkNumVerticesXZ.x, triangles);
            JobHandle tJobHandle = tJob.ScheduleParallel(totalChunkQuads, JobWorkerCount - 1, vJobHandle);
            
            //Uvs
            using NativeArray<float2> uvs = AllocNtvAry<float2>(numVertices);
            JUvs2 uJob = new (kwTerrainData.TerrainNumVerticesXZ.x, uvs);
            JobHandle uJobHandle = uJob.ScheduleParallel(numVertices, JobWorkerCount - 1, tJobHandle);

            //Ordered Array by Chunks
            GridData gridData = new (kwTerrainData.TerrainNumQuadsXZ, 1, kwTerrainData.ChunkSize);
            using NativeArray<float2> uvsOrdered = AllocNtvAry<float2>(numVertices);
            JobHandle uvsOrderJobHandle = uvsOrdered.OrderNativeArrayByChunk(uvs, gridData, uJobHandle);
            uvsOrderJobHandle.Complete();

            int numChunk = cmul(kwTerrainData.TerrainNumQuadsXZ / kwTerrainData.ChunkSize);
            UnityEngine.Debug.Log($"Vertices per chunk = {totalChunkVertices}");
            
            int[] tri = triangles.ToArray();
            for (int i = 0; i < numChunk; i++)
            {
                Mesh chunkMesh = chunks[i].GetComponent<MeshFilter>().mesh;
                
                chunkMesh.SetVertices(verticesTemp.Reinterpret<Vector3>());
                chunkMesh.SetTriangles(tri,0);
                chunkMesh.SetUVs(0,uvsOrdered.GetSubArray(i * totalChunkVertices, totalChunkVertices).Reinterpret<Vector2>());
                
                chunkMesh.RecalculateNormals();
                chunkMesh.RecalculateTangents();
            }
            
        }
        private void GenerateBigMap()
        {
            mesh = new Mesh 
            {
                name = "Procedural Mesh",
                bounds = new Bounds(Vector3.zero, new Vector3(MapSize.x,0,MapSize.y)),
            };
            GetComponent<MeshFilter>().mesh = mesh;
            
            int numVertices = cmul(kwTerrainData.TerrainNumVerticesXZ);
            
            //UnityEngine.Debug.Log($"numVertices = {numVertices}");
            //UnityEngine.Debug.Log($"total quads = {cmul(kwTerrainData.TerrainNumQuadsXZ)}");
            //UnityEngine.Debug.Log($"num quads = {kwTerrainData.TerrainNumQuadsXZ}; num Vertices = {kwTerrainData.TerrainNumVerticesXZ}");
            
            //Vertices
            using NativeArray<float3> verticesTemp = AllocNtvAry<float3>(numVertices);
            JVerticesPosition2 vJob = new (kwTerrainData.TerrainNumVerticesXZ.x, verticesTemp);
            JobHandle vJobHandle = vJob.ScheduleParallel(numVertices, JobWorkerCount - 1, default);
            
            //Triangles
            using NativeArray<int> triangles = AllocNtvAry<int>(cmul(kwTerrainData.TerrainNumQuadsXZ) * 6);
            JTriangles2 tJob = new (kwTerrainData.TerrainNumQuadsXZ.x,kwTerrainData.TerrainNumVerticesXZ.x, triangles);
            JobHandle tJobHandle = tJob.ScheduleParallel(cmul(kwTerrainData.TerrainNumQuadsXZ), JobWorkerCount - 1, vJobHandle);

            //Uvs
            using NativeArray<float2> uvs = AllocNtvAry<float2>(numVertices);
            JUvs2 uJob = new (kwTerrainData.TerrainNumVerticesXZ.x, uvs);
            JobHandle uJobHandle = uJob.ScheduleParallel(numVertices, JobWorkerCount - 1, tJobHandle);
            uJobHandle.Complete();

            mesh.vertices = verticesTemp.Reinterpret<Vector3>().ToArray();
            mesh.triangles = triangles.ToArray();
            mesh.uv = uvs.Reinterpret<Vector2>().ToArray();
            
            mesh.RecalculateNormals();
            mesh.RecalculateTangents();
        }
    }
}
