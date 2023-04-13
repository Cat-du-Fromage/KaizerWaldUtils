using Unity.Burst;
using Unity.Collections;
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

        private void Awake()
        {
            Settings = new TerrainSettings(NumQuadPerLine, NumChunkXY, NoiseSettings);
            NumQuadPerLine = Settings.NumQuadPerLine;
            NumChunkXY = Settings.NumChunkAxis;
        }

        private void Start()
        {
            Chunks = CreateChunks(gameObject, Settings, true, DefaultMaterial);
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
   