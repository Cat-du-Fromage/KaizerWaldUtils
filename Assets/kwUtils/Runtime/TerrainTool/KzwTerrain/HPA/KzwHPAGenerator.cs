using System;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

using static System.Array;
using static Unity.Mathematics.math;
using static KWUtils.KWmath;
using static KWUtils.KWGrid;
using static KWUtils.KWChunk;

using static Unity.Jobs.LowLevel.Unsafe.JobsUtility;
using static Unity.Collections.Allocator;
using static Unity.Collections.NativeArrayOptions;

using int2 = Unity.Mathematics.int2;

namespace KWUtils
{
    [RequireComponent(typeof(KzwTerrainGenerator))]
    public partial class KzwHPAGenerator : MonoBehaviour
    {
        private KzwTerrainGenerator terrain;
        
        private GateGrid GatesGridSystem;
        // GRILLE DES OBSTACLES
        private GenericChunkedGrid<bool> obstaclesGrid;

        private void Awake()
        {
            bool terrainExist = TryGetComponent(out terrain);
        }

        private void Start()
        {
            GatesGridSystem = new GateGrid(terrain.Settings.ChunkQuadsPerLine, terrain.Settings.NumChunkAxis);
        }
// =====================================================================================================================
// --- Cluster Gates ---
// =====================================================================================================================
    
    }
}