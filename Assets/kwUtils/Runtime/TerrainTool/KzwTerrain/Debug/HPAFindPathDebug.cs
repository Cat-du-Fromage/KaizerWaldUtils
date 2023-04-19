using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;


#if UNITY_EDITOR
namespace KWUtils
{
    public partial class HPAFindPathDebug : MonoBehaviour
    {
        private Camera mainCamera;
        private Mouse mouse;
        private Ray cameraRay => mainCamera.ScreenPointToRay(mouse.position.ReadValue());

        private void Awake()
        {
            mainCamera = Camera.main;
            mouse = Mouse.current;
        }

        private void Update()
        {
            StartToken();
            EndToken();
        }

        private void StartToken()
        {
            if (mouse.leftButton.wasReleasedThisFrame) return;
            if(Physics.Raycast(cameraRay,out RaycastHit hit, Mathf.Infinity, LayerMask.NameToLayer("Terrain")))
            {
                Vector3 hitPosition = hit.point;
            }
        }
        
        private void EndToken()
        {
            if (mouse.rightButton.wasReleasedThisFrame) return;
            if(Physics.Raycast(cameraRay,out RaycastHit hit, Mathf.Infinity, LayerMask.NameToLayer("Terrain")))
            {
                Vector3 hitPosition = hit.point;
            }
        }
    }
}
#endif