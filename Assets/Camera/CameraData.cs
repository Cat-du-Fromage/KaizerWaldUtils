using System;
using UnityEngine;

namespace KaizerWaldCode.RTTCamera
{
    [Serializable]
    public struct CameraData
    {
        public int RotationSpeed;
        public int BaseMoveSpeed;
        public int ZoomSpeed;
        public int SprintModifier;
        
        public float MaxRotation;
        public float MinRotation;

        public float MinZoom;
        
        public readonly int SprintSpeed => BaseMoveSpeed * SprintModifier;
        
        public CameraData(int rotationSpeed, int baseMoveSpeed, int zoomSpeed, int sprintModifier, float max = 45f, float min = -60f, float minZoom = 0)
        {
            RotationSpeed = Mathf.Max(rotationSpeed,1);
            BaseMoveSpeed = Mathf.Max(baseMoveSpeed,1);
            ZoomSpeed = Mathf.Max(zoomSpeed,1);
            SprintModifier = Mathf.Max(sprintModifier,1);
            MaxRotation = max;
            MinRotation = min;
            MinZoom = minZoom;
        }
    }
}