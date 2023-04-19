using JetBrains.Annotations;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;

using static KWUtils.KWmath;
using static KWUtils.KWGrid;
using static Unity.Jobs.LowLevel.Unsafe.JobsUtility;
using static Unity.Mathematics.math;

using static UnityEngine.Mesh;
using static Unity.Collections.Allocator;
using static Unity.Collections.NativeArrayOptions;

using Mesh = UnityEngine.Mesh;

namespace KWUtils
{
    public static class ChunkBuilderUtils
    {
        public static GameObject[] CreateChunks(GameObject terrain, TerrainSettings settings, bool parented, [CanBeNull] Material defaultMaterial = null)
        {
            Mesh[] chunkMeshes = GenerateChunksMeshes(settings);
            using NativeArray<float3> positions = GetChunksPosition(settings.NumQuadPerLine, settings.NumChunkAxis);
            GameObject[] chunks = BuildChunks();
            UpdateCollider(chunks);
            return chunks;
            // -------------------------------------------------------------------------------------------------------
            // INTERNAL METHODS
            // -------------------------------------------------------------------------------------------------------

            // Create and Name Chunks
            GameObject[] BuildChunks()
            {
                GameObject chunkPrefab = GameObject.CreatePrimitive(PrimitiveType.Plane);
                GameObject[] chunks = new GameObject[settings.ChunksCount];
                for (int i = 0; i < chunks.Length; i++)
                {
                    chunks[i] = parented 
                        ? Object.Instantiate(chunkPrefab, positions[i], Quaternion.identity, terrain.transform) 
                        : Object.Instantiate(chunkPrefab, positions[i], Quaternion.identity);

                    chunks[i].name = $"Chunk_{i}";
                    chunks[i].layer = LayerMask.NameToLayer("Terrain");
                }
                Object.Destroy(chunkPrefab);
                return chunks;
            }

            // Update Mesh Collider and Mesh Renderer
            void UpdateCollider(GameObject[] chunks)
            {
                for (int i = 0; i < chunks.Length; i++)
                {
                    GameObject chunk = chunks[i];
                    Mesh chunkMesh = chunkMeshes[i];
                    //chunkMesh.RecalculateNormals();
                    //chunkMesh.RecalculateTangents();
                    chunkMesh.RecalculateBounds();
                    
                    chunk.GetComponent<MeshFilter>().mesh = chunkMeshes[i];
                    chunk.GetComponent<MeshCollider>().sharedMesh = chunkMesh;
                    chunk.GetComponent<MeshRenderer>().ResetBounds();
                    if (defaultMaterial == null) return;
                    chunk.GetComponent<MeshRenderer>().material = defaultMaterial;
                }
            }
        }

// =====================================================================================================================
        // Chunks Builder
        // =================================================================================================================
        /// <summary>
        /// Generate Meshes according to settings
        /// </summary>
        /// <returns>Meshes constructed</returns>
        public static Mesh[] GenerateChunksMeshes(TerrainSettings settings)
        {
            // cache values
            int numChunks = settings.ChunksCount;
            int2 numChunksXY = settings.NumChunkAxis;
            int verticesCount = settings.ChunkVerticesCount;
            int triIndicesCount = settings.ChunkIndicesCount;

            Mesh[] chunkMeshes = new Mesh[numChunks];
            MeshDataArray meshDataArray = AllocateWritableMeshData(numChunks);

            using (NativeArray<float> noiseMap = new(verticesCount, TempJob, UninitializedMemory))
            {
                NativeList<JobHandle> jobHandles = new(numChunks, Temp);
                NativeArray<VertexAttributeDescriptor> vertexAttributes = KWmesh.InitializeVertexAttribute();
                for (int i = 0; i < numChunks; i++)
                {
                    chunkMeshes[i] = new Mesh { name = $"ChunkMesh_{i}" };
                    //we need to "re-center" the coord if we want (0,0,0) to be the center of the map
                    int2 coordCentered = GetXY2(i, numChunksXY.x) - (numChunksXY / 2);
                    MeshData meshData = InitMeshDataAt(i, vertexAttributes);

                    JobHandle dependency = (i == 0) ? default : jobHandles[i - 1];
                    JobHandle meshJobHandle = CreateMesh(meshData, coordCentered, noiseMap, dependency);

                    jobHandles.Add(meshJobHandle);
                }

                jobHandles[^1].Complete();
                SetSubMeshes();
            }

            ApplyAndDisposeWritableMeshData(meshDataArray, chunkMeshes);
            return chunkMeshes;

            // -------------------------------------------------------------------------------------------------------
            // INTERNAL METHODS
            // -------------------------------------------------------------------------------------------------------

            void SetSubMeshes()
            {
                SubMeshDescriptor descriptor = new(0, triIndicesCount) { vertexCount = verticesCount };
                for (int i = 0; i < numChunks; i++)
                {
                    meshDataArray[i].SetSubMesh(0, descriptor, MeshUpdateFlags.DontRecalculateBounds);
                }
            }

            JobHandle CreateMesh(MeshData meshData, in int2 coord, NativeArray<float> noiseMap, JobHandle dependency)
            {
                JobHandle noiseJh = ChunkMeshBuilderUtils.SetNoiseJob(settings, settings, coord, noiseMap, dependency);
                JobHandle meshJh = ChunkMeshBuilderUtils.SetMeshJob(settings, meshData, noiseMap, noiseJh);
                JobHandle normalsJh = ChunkMeshBuilderUtils.SetNormalsJob(settings, meshData, meshJh);
                JobHandle tangentsJh = ChunkMeshBuilderUtils.SetTangentsJob(settings, meshData, normalsJh);

                return tangentsJh;
            }

            MeshData InitMeshDataAt(int index, NativeArray<VertexAttributeDescriptor> vertexAttributes)
            {
                MeshData meshData = meshDataArray[index];
                meshData.subMeshCount = 1;
                meshData.SetVertexBufferParams(verticesCount, vertexAttributes);
                meshData.SetIndexBufferParams(triIndicesCount, IndexFormat.UInt16);
                return meshData;
            }
        }

        // =============================================================================================================

        // =============================================================================================================
        /// <summary>
        /// Reposition chunks according to their index and to the center of the map
        /// </summary>
        /// <param name="numQuadPerLine">chunk size (by the number of quads)</param>
        /// <param name="numChunkXY">number of chunks on axis X and Y</param>
        public static NativeArray<float3> GetChunksPosition(int numQuadPerLine, int2 numChunkXY)
        {
            NativeArray<float3> positions = new(cmul(numChunkXY), TempJob, UninitializedMemory);
            JGetChunkPositions job = new()
            {
                ChunkQuadsPerLine = numQuadPerLine,
                NumChunksAxis = numChunkXY,
                Positions = positions
            };
            job.ScheduleParallel(positions.Length, JobWorkerCount - 1, default).Complete();
            return positions;
        }

// =====================================================================================================================
// --- JOBS ---
// =====================================================================================================================
        [BurstCompile]
        private struct JGetChunkPositions : IJobFor
        {
            [ReadOnly] public int ChunkQuadsPerLine;
            [ReadOnly] public int2 NumChunksAxis;

            [WriteOnly, NativeDisableParallelForRestriction]
            public NativeArray<float3> Positions;

            public void Execute(int index)
            {
                float halfSizeChunk = ChunkQuadsPerLine / 2f;
                int2 halfNumChunks = NumChunksAxis / 2; //we don't want 0.5!
                int2 coord = GetXY2(index, NumChunksAxis.x) - halfNumChunks;

                float2 positionOffset = mad(coord, ChunkQuadsPerLine, halfSizeChunk);
                //Case the is only 1 chunk halfNumChunks.x/y == 0
                float positionX = select(positionOffset.x, 0, halfNumChunks.x == 0);
                float positionY = select(positionOffset.y, 0, halfNumChunks.y == 0);

                Positions[index] = new float3(positionX, 0, positionY);
            }
        }
    }
}
