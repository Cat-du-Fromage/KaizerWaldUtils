using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;

using static UnityEngine.Mesh;
using static Unity.Mathematics.math;
using static KWUtils.KWmath;
using static KWUtils.KWGrid;
using static KWUtils.NativeCollectionExt;
using static Unity.Jobs.LowLevel.Unsafe.JobsUtility;
using int2 = Unity.Mathematics.int2;

namespace KWUtils.KwTerrain
{
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
    public class TerrainGenerator : MonoBehaviour
    {
        [SerializeField] private Material defaultChunkMaterial;
        
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
            //CreateChunks();
            
            GenerateBigMap();
            //GenerateChunks();
        }

        private void CreateChunks()
        {
            int numChunk = cmul(MapSize / ChunkSize);
            chunks = new Dictionary<int, GameObject>(numChunk);
            
            for (int i = 0; i < numChunk; i++)
            {
                int2 PositionInWorld = GetXY2(i,(MapSize / ChunkSize).x);
                chunks.TryAdd(i, new GameObject($"Chunk_{i}", typeof(TerrainChunkComponent)));

                float2 center = (PositionInWorld * ChunkSize) + (ChunkSize / 2);
                Mesh chunkMesh = new Mesh 
                {
                    name = $"ChunkMesh_{i}",
                    bounds = new Bounds(new Vector3(center.x,0,center.y), new Vector3(ChunkSize,0,ChunkSize)),
                };
                chunks[i].transform.position = new Vector3(PositionInWorld.x*(ChunkSize), 0, PositionInWorld.y*(ChunkSize));
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

        public void GenerateChunks()
        {
            int numVertices = cmul(kwTerrainData.TerrainVerticesXZ);
            //int totalChunkQuads = cmul(kwTerrainData.NumChunkXZ);
            
            int numChunk = cmul(kwTerrainData.NumChunkXZ);
            int totalChunkVertices = cmul(kwTerrainData.ChunkVerticesXZ);
            int numSharedVertices = cmul(kwTerrainData.ChunkVerticesXZ) * numChunk;

            //Vertices
            using NativeArray<float3> verticesTemp = AllocNtvAry<float3>(totalChunkVertices);
            JVerticesPosition2 vJob = new (kwTerrainData.ChunkVerticesXZ.x, verticesTemp);
            JobHandle vJobHandle = vJob.ScheduleParallel(totalChunkVertices, JobWorkerCount - 1, default);

            //Triangles : Use Quads!
            using NativeArray<int> triangles = AllocNtvAry<int>(numChunk * 6);
            JTriangles2 tJob = new (kwTerrainData.NumChunkXZ.x, kwTerrainData.ChunkVerticesXZ.x, triangles);
            JobHandle tJobHandle = tJob.ScheduleParallel(numChunk, JobWorkerCount - 1, vJobHandle);
            
            //Uvs
            using NativeArray<float2> uvs = AllocNtvAry<float2>(numVertices);
            JUvs2 uJob = new (kwTerrainData.TerrainVerticesXZ, uvs);
            JobHandle uJobHandle = uJob.ScheduleParallel(numVertices, JobWorkerCount - 1, tJobHandle);

            //Ordered Array by Chunks
            //GridData gridData = new (kwTerrainData.TerrainNumVerticesXZ, 1, kwTerrainData.ChunkSize);
            using NativeArray<float2> uvsOrdered = AllocNtvAry<float2>(numSharedVertices);
            JobHandle uvsOrderJobHandle = uvsOrdered.SharedOrderNativeArrayByChunk(uvs, kwTerrainData, uJobHandle);
            uvsOrderJobHandle.Complete();

            
            UnityEngine.Debug.Log($"Vertices per chunk = {totalChunkVertices}");
            
            int[] tri = triangles.ToArray();
            Mesh chunkMesh;
            for (int i = 0; i < numChunk; i++)
            {
                chunkMesh = chunks[i].GetComponent<MeshFilter>().mesh;
                
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
                indexFormat = IndexFormat.UInt16,
                name = "Procedural Mesh",
                bounds = new Bounds(Vector3.zero, new Vector3(MapSize.x,0,MapSize.y)),
            };
            GetComponent<MeshFilter>().mesh = mesh;
            GetComponent<Renderer>().material = defaultChunkMaterial;
            int numVertices = cmul(kwTerrainData.TerrainVerticesXZ);
            
            //Vertices
            using NativeArray<float3> verticesTemp = AllocNtvAry<float3>(numVertices);
            JVerticesPosition2 vJob = new (kwTerrainData.TerrainVerticesXZ.x, verticesTemp);
            JobHandle vJobHandle = vJob.ScheduleParallel(numVertices, JobWorkerCount - 1, default);
            
            //Triangles
            using NativeArray<int> triangles = AllocNtvAry<int>(cmul(kwTerrainData.TerrainSizeXZ) * 6);
            JTriangles2 tJob = new (kwTerrainData.TerrainSizeXZ.x,kwTerrainData.TerrainVerticesXZ.x, triangles);
            JobHandle tJobHandle = tJob.ScheduleParallel(cmul(kwTerrainData.TerrainSizeXZ), JobWorkerCount - 1, vJobHandle);

            //Uvs
            using NativeArray<float2> uvs = AllocNtvAry<float2>(numVertices);
            JUvs2 uJob = new (kwTerrainData.TerrainVerticesXZ, uvs);
            JobHandle uJobHandle = uJob.ScheduleParallel(numVertices, JobWorkerCount - 1, tJobHandle);
            uJobHandle.Complete();

            mesh.SetVertices(verticesTemp.Reinterpret<Vector3>());
            //mesh.vertices = verticesTemp.Reinterpret<Vector3>().ToArray();
            //mesh.triangles = triangles.ToArray();
            mesh.SetTriangles(triangles.ToArray(), 0);
            //mesh.uv = uvs.Reinterpret<Vector2>().ToArray();
            mesh.SetUVs(0,uvs.Reinterpret<Vector2>());
            
            mesh.RecalculateNormals();
            mesh.RecalculateTangents();
            
            
        }
    }
}
