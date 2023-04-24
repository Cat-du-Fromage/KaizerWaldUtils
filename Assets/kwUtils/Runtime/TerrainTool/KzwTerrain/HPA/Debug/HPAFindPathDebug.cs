using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using static UnityEngine.Physics;

#if UNITY_EDITOR
namespace KWUtils
{
    public partial class HPAFindPathDebug : MonoBehaviour
    {
        [SerializeField] private GameObject PrefabStart, PrefabEnd;
        private Camera mainCamera;
        private Mouse mouse;
        private const int TerrainLayer = 1 << 8;
        private Ray cameraRay => mainCamera.ScreenPointToRay(mouse.position.ReadValue());

        private void Awake()
        {
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
        }

        private void SetToken(GameObject prefab, ButtonControl button)
        {
            if (!button.wasReleasedThisFrame) return;
            if (!Raycast(cameraRay, out RaycastHit hit, Mathf.Infinity, TerrainLayer)) return;
            Vector3 hitPosition = hit.point;
            prefab.transform.position = hitPosition;
            
            if (prefab.activeSelf) return;
            prefab.SetActive(true);
        }
    }
}
#endif