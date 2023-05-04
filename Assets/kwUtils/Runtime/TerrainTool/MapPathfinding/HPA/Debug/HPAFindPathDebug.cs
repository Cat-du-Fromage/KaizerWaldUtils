using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

using static UnityEngine.Physics;
using static KWUtils.KWGrid;

#if UNITY_EDITOR
namespace KWUtils
{
    public partial class HPAFindPathDebug : MonoBehaviour
    {
        private KzwTerrainGenerator terrain;
        private KzwHPAGenerator gridSystem;
        [SerializeField] private GameObject PrefabStart, PrefabEnd;
        private bool calculatePath = false;
        private Camera mainCamera;
        private Mouse mouse;

        private List<int> pathDebug;

        private const int TerrainLayer = 1 << 8;
        private Ray cameraRay => mainCamera.ScreenPointToRay(mouse.position.ReadValue());

        private void Awake()
        {
            TryGetComponent(out terrain);
            TryGetComponent(out gridSystem);
            mainCamera = Camera.main;
            mouse = Mouse.current;
            PrefabStart = Instantiate(PrefabStart);
            PrefabEnd = Instantiate(PrefabEnd);
        }

        private void Start()
        {
            PrefabStart.SetActive(false);
            PrefabEnd.SetActive(false);
        }

        private void Update()
        {
            SetToken(PrefabStart, mouse.leftButton);
            SetToken(PrefabEnd, mouse.rightButton);
            DebugCalculatePath();
        }

        private void OnDrawGizmos()
        {
            if (pathDebug == null) return;
            Gizmos.color = Color.yellow;
            foreach (int chunkIndex in pathDebug)
            {
                Vector3 chunkCenter = terrain.Grid.GetChunkCenter(chunkIndex);
                Gizmos.DrawCube(chunkCenter, Vector3.one);
            }
        }

        private void DebugCalculatePath()
        {
            if (!calculatePath) return;
            if (!PrefabStart.activeSelf || !PrefabEnd.activeSelf)
            {
                calculatePath = false;
                return;
            }

            int startIndex = terrain.Grid.GetChunkIndexFromPosition(PrefabStart.transform.position);
            int endIndex = terrain.Grid.GetChunkIndexFromPosition(PrefabEnd.transform.position);
            using NativeArray<bool4> chunkSides = gridSystem.GatesGridSystem.ChunkSides.ToNativeArray();
            using NativeList<int> chunkPath = new (terrain.Settings.ChunksCount, Allocator.TempJob);
            JGetChunkPath job = new JGetChunkPath
            (
                startIndex,
                endIndex,
                terrain.Settings.NumChunkAxis,
                chunkSides,
                chunkPath
            );
            JobHandle jh = job.Schedule();
            jh.Complete();
            pathDebug = new List<int>(chunkPath.AsArray().ToArray());
            calculatePath = false;
        }

        private void SetToken(GameObject prefab, ButtonControl button)
        {
            if (!button.wasReleasedThisFrame) return;
            if (!Raycast(cameraRay, out RaycastHit hit, Mathf.Infinity, TerrainLayer)) return;
            prefab.transform.position = hit.point;
            calculatePath = true;
            if (prefab.activeSelf) return;
            prefab.SetActive(true);
        }
    }
}
#endif