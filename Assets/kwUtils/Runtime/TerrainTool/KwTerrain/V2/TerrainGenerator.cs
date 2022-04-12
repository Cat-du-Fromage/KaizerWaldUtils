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

namespace KWUtils.KwTerrain
{
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
    public class TerrainGenerator : MonoBehaviour
    {
        //Data
        public int2 MapSize = int2(1);
        
        public int ChunkSize = 1;
        
        private int numVertices;
        
        private Mesh mesh;

        private void OnValidate()
        {
            CheckValues();
        }

        private void Awake()
        {
            CheckValues();
            numVertices = cmul(MapSize);
            
            mesh = new Mesh 
            {
                name = "Procedural Mesh",
                bounds = new Bounds(Vector3.zero, new Vector3(MapSize.x,0,MapSize.y)),
            };
            GenerateMesh();
            GetComponent<MeshFilter>().mesh = mesh;
        }

        /// <summary>
        /// Check All value are Pow2
        /// </summary>
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
        
        private void GenerateMesh()
        {
            //MeshDataArray meshDataArray = AllocateWritableMeshData(1);
            //MeshData meshData = meshDataArray[0];
            
            //Actual Job
            GetDatas();
            
            //ApplyAndDisposeWritableMeshData(meshDataArray, mesh);
        }

        public void GetDatas()
        {
            //Vertices
            using NativeArray<float3> verticesTemp = AllocNtvAry<float3>(numVertices);
            JVerticesPosition vJob = new JVerticesPosition(MapSize.x, verticesTemp);
            JobHandle vJobHandle = vJob.ScheduleParallel(numVertices, JobWorkerCount - 1, default);
            //Triangles
            using NativeArray<int> triangles = AllocNtvAry<int>(cmul(MapSize - 1) * 6);
            JTriangles tJob = new JTriangles(MapSize.x, triangles);
            JobHandle tJobHandle = tJob.ScheduleParallel(cmul(MapSize-1), JobWorkerCount - 1, vJobHandle);

            using NativeArray<float2> uvs = AllocNtvAry<float2>(numVertices);
            JUvs uJob = new JUvs(MapSize.x, uvs);
            JobHandle uJobHandle = uJob.ScheduleParallel(numVertices, JobWorkerCount - 1, tJobHandle);
            uJobHandle.Complete();
            
            UnityEngine.Debug.Log($"vert = {verticesTemp.Length}");
            UnityEngine.Debug.Log($"vert = {triangles.Length}");
            
            mesh.vertices = (verticesTemp.Reinterpret<Vector3>().ToArray());
            mesh.triangles = (triangles.ToArray());
            mesh.uv = uvs.Reinterpret<Vector2>().ToArray();
            mesh.RecalculateNormals();
            mesh.RecalculateTangents();
        }
    }
}
