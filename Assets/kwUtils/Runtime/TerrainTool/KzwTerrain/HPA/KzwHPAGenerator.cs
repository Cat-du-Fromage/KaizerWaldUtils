using System;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;
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
        
        public GateGrid GatesGridSystem { get; private set; }
        // GRILLE DES OBSTACLES
        private GenericChunkedGrid<bool> obstaclesGrid;

        private void Awake()
        {
            bool terrainExist = TryGetComponent(out terrain);
        }

        private void Start()
        {
            int chunkQuadsPerLine = terrain.Settings.ChunkQuadsPerLine;
            GatesGridSystem = new GateGrid(chunkQuadsPerLine, terrain.Settings.NumChunkAxis);
            obstaclesGrid = new GenericChunkedGrid<bool>(terrain.Settings.NumQuadsAxis, chunkQuadsPerLine);
        }

        private void Update()
        {
            
        }
// =====================================================================================================================
// --- Cluster Gates ---
// =====================================================================================================================
    
    }
}