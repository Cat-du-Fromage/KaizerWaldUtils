using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;

using static KWUtils.KWmath;
using static KWUtils.KWGrid;
using static KWUtils.ChunkBuilderUtils;

using static Unity.Jobs.LowLevel.Unsafe.JobsUtility;
using static Unity.Mathematics.math;

using static UnityEngine.Mesh;
using static Unity.Collections.Allocator;
using static Unity.Collections.NativeArrayOptions;

using Mesh = UnityEngine.Mesh;

namespace KWUtils
{
    public class KzwTerrainGenerator : MonoBehaviour
    {
        [SerializeField] private Material DefaultMaterial;
        [field: SerializeField] public int NumQuadPerLine { get; private set; }
        [field: SerializeField] public int2 NumChunkXY { get; private set; }
        [field: SerializeField] public NoiseData NoiseSettings { get; private set; }
        [field: SerializeField] public TerrainSettings Settings { get; private set; }
        
        public GameObject[] Chunks { get; private set; }

        public GenericChunkedGrid<GridCell> Grid { get; private set; }

        private void Awake()
        {
            Settings = new TerrainSettings(NumQuadPerLine, NumChunkXY, NoiseSettings);
            NumQuadPerLine = Settings.NumQuadPerLine;
            NumChunkXY = Settings.NumChunkAxis;

            Grid = new GenericChunkedGrid<GridCell>(Settings.NumQuadsAxis, Settings.NumQuadPerLine);
        }

        private void Start()
        {
            Chunks = CreateChunks(gameObject, Settings, true, DefaultMaterial);
            Grid.CopyFrom(PopulateGridCell(Settings));
        }

        private NativeArray<GridCell> PopulateGridCell(TerrainSettings settings)
        {
            //const int NumVerticesPerQuad = 4;
            using MeshDataArray meshDataArray = AcquireReadOnlyMeshData(Chunks.GetMeshesComponent());
            using NativeArray<float3> orderedVertices = GetOrderedVertices(Chunks, meshDataArray, settings);
                
            NativeArray<GridCell> cellsArray = new (settings.QuadCount, TempJob, UninitializedMemory);
            /*
            NativeArray<float3> cellVertices = new (4, Temp, UninitializedMemory);
            for (int cellIndex = 0; cellIndex < cellsArray.Length; cellIndex++)
            {
                int2 cellCoord = GetXY2(cellIndex, terrainQuadsXY.x);
                for (int vertexIndex = 0; vertexIndex < NumVerticesPerQuad; vertexIndex++)
                {
                    int2 vertexCoord = GetXY2(vertexIndex, verticesWidth);
                    int index = mad(cellCoord.y + vertexCoord.y, numVerticesX,cellCoord.x + vertexCoord.x);
                    cellVertices[vertexIndex] = verticesNtv[index];
                }
                cellsArray[cellIndex] = new GridCell(terrainQuadsXY, cellCoord, cellVertices);
            }
            */
            JCellOrderByChunk job = new JCellOrderByChunk
            {
                NumVerticesX = settings.NumVerticesX,
                TerrainQuadsXY = settings.NumQuadsAxis,
                OrderedVertices = orderedVertices,
                CellsArray = cellsArray,
            };
            job.ScheduleParallel(settings.ChunkVerticesCount, JobWorkerCount - 1, default).Complete();
            return cellsArray;
        }
        
        [BurstCompile(CompileSynchronously = false)]
        private partial struct JCellOrderByChunk : IJobFor
        {
            [ReadOnly] public int NumVerticesX;
            [ReadOnly] public int2 TerrainQuadsXY;
            
            [ReadOnly,NativeDisableParallelForRestriction, NativeDisableContainerSafetyRestriction] 
            public NativeArray<float3> OrderedVertices;
            [WriteOnly,NativeDisableParallelForRestriction, NativeDisableContainerSafetyRestriction] 
            public NativeArray<GridCell> CellsArray;
            
            public void Execute(int index)
            {
                int2 cellCoord = GetXY2(index, TerrainQuadsXY.x);
                CellsArray[index] = GetCell(cellCoord);
            }

            private GridCell GetCell(in int2 cellCoord)
            {
                int i0 = GetIndex(cellCoord, NumVerticesX);
                int i1 = GetIndex(cellCoord + new int2(1,0), NumVerticesX);
                int i2 = GetIndex(cellCoord + new int2(0,1), NumVerticesX);
                int i3 = GetIndex(cellCoord + new int2(1,1), NumVerticesX);
                return new GridCell(TerrainQuadsXY, cellCoord, OrderedVertices[i0], OrderedVertices[i1], OrderedVertices[i2], OrderedVertices[i3]);
            }
        }
        
// =====================================================================================================================
// --- Reorder Vertices ---
// =====================================================================================================================
        
        private NativeArray<float3> GetOrderedVertices(GameObject[] chunks, MeshDataArray meshDataArray, TerrainSettings settings)
        {
            NativeArray<float3> verticesNtv = new(settings.MapVerticesCount, TempJob, UninitializedMemory);
            NativeArray<JobHandle> jobHandles = new(chunks.Length, Temp, UninitializedMemory);

            for (int chunkIndex = 0; chunkIndex < chunks.Length; chunkIndex++)
            {
                int2 chunkCoord = GetXY2(chunkIndex, settings.NumChunkAxis.x);
                JReorderMeshVertices job = new JReorderMeshVertices()
                {
                    TerrainNumVertexPerLine = settings.NumVerticesX,
                    ChunkNumVertexPerLine = settings.ChunkVerticesPerLine,
                    ChunkCoord = chunkCoord,
                    ChunkPosition = chunks[chunkIndex].transform.position,
                    MeshVertices = meshDataArray[chunkIndex].GetVertexData<float3>(stream: 0),
                    OrderedVertices = verticesNtv
                };
                jobHandles[chunkIndex] = job.ScheduleParallel(settings.ChunkVerticesCount,JobWorkerCount - 1,default);
            }
            JobHandle.CompleteAll(jobHandles);
            return verticesNtv;
        }

        [BurstCompile]
        private struct JReorderMeshVertices : IJobFor
        {
            [ReadOnly] public int TerrainNumVertexPerLine;
            [ReadOnly] public int ChunkNumVertexPerLine;
            [ReadOnly] public int2 ChunkCoord;
            [ReadOnly] public float3 ChunkPosition;

            [ReadOnly, NativeDisableParallelForRestriction, NativeDisableContainerSafetyRestriction]
            public NativeArray<float3> MeshVertices;
        
            [WriteOnly, NativeDisableParallelForRestriction, NativeDisableContainerSafetyRestriction]
            public NativeArray<float3> OrderedVertices;
        
            public void Execute(int index)
            {
                int2 cellCoord = GetXY2(index, ChunkNumVertexPerLine);
                bool2 skipDuplicate = new (ChunkCoord.x > 0 && cellCoord.x == 0, ChunkCoord.y > 0 && cellCoord.y == 0);
                if (any(skipDuplicate)) return;
                int chunkNumQuadPerLine = ChunkNumVertexPerLine - 1;
                int2 offset = ChunkCoord * chunkNumQuadPerLine;
                int2 fullTerrainCoord = cellCoord + offset;
                int fullMapIndex = GetIndex(fullTerrainCoord, TerrainNumVertexPerLine);
                OrderedVertices[fullMapIndex] = ChunkPosition + MeshVertices[index];
            }
        }
        

// =====================================================================================================================
// EDITOR
// =====================================================================================================================
        private void OnValidate()
        {
            Settings = new TerrainSettings(NumQuadPerLine, NumChunkXY, NoiseSettings);
            NumQuadPerLine = Settings.NumQuadPerLine;
            NumChunkXY = Settings.NumChunkAxis;
        }
    }
}
   