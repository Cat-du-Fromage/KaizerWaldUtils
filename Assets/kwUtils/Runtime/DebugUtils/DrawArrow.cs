using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using static UnityEngine.Quaternion;

namespace KWUtils.Debug
{
    /// Utility class for drawing arrows
    /// https://forum.unity.com/threads/debug-drawarrow.85980/
    public static class DrawArrow
    {
        public static void ForGizmo(Vector3 pos, Vector3 direction, float arrowHeadLength = 0.25f, float arrowHeadAngle = 20.0f)
        {
            Gizmos.DrawRay(pos, direction);
            //Vector3 trueZero = new Vector3(Vector3.kEpsilon,Vector3.kEpsilon,Vector3.kEpsilon);
            Quaternion rotation = direction != Vector3.zero ? LookRotation(direction) : identity;
            
            Vector3 right = rotation * Euler(0, 180 + arrowHeadAngle, 0) * new Vector3(0, 0, 1);
            Vector3 left  = rotation * Euler(0, 180 - arrowHeadAngle, 0) * new Vector3(0, 0, 1);

            Gizmos.DrawRay(pos + direction, right * arrowHeadLength);
            Gizmos.DrawRay(pos + direction, left * arrowHeadLength);
        }

        public static void ForGizmo(Vector3 pos, Vector3 direction, Color color, float arrowHeadLength = 0.25f, float arrowHeadAngle = 20.0f)
        {
            Gizmos.color = color;
            Gizmos.DrawRay(pos, direction);
            Quaternion rotation = direction != Vector3.zero ? LookRotation(direction) : identity;

            Vector3 right = rotation * Euler(0, 180 + arrowHeadAngle, 0) * new Vector3(0, 0, 1);
            Vector3 left = rotation * Euler(0, 180 - arrowHeadAngle, 0) * new Vector3(0, 0, 1);
            Gizmos.DrawRay(pos + direction, right * arrowHeadLength);
            Gizmos.DrawRay(pos + direction, left * arrowHeadLength);
        }

        public static void ForDebug(Vector3 pos, Vector3 direction, float arrowHeadLength = 0.25f, float arrowHeadAngle = 20.0f)
        {
            UnityEngine.Debug.DrawRay(pos, direction);
            Quaternion rotation = direction != Vector3.zero ? LookRotation(direction) : identity;
            Vector3 right = rotation * Euler(0, 180 + arrowHeadAngle, 0) * new Vector3(0, 0, 1);
            Vector3 left = rotation * Euler(0, 180 - arrowHeadAngle, 0) * new Vector3(0, 0, 1);
            UnityEngine.Debug.DrawRay(pos + direction, right * arrowHeadLength);
            UnityEngine.Debug.DrawRay(pos + direction, left * arrowHeadLength);
        }
        
        public static void ForDebug(Vector3 pos, Vector3 direction, Color color, float arrowHeadLength = 0.25f, float arrowHeadAngle = 20.0f)
        {
            UnityEngine.Debug.DrawRay(pos, direction, color);
            Quaternion rotation = direction != Vector3.zero ? LookRotation(direction) : identity;
            Vector3 right = rotation * Euler(0, 180 + arrowHeadAngle, 0) * new Vector3(0, 0, 1);
            Vector3 left = rotation * Euler(0, 180 - arrowHeadAngle, 0) * new Vector3(0, 0, 1);
            UnityEngine.Debug.DrawRay(pos + direction, right * arrowHeadLength, color);
            UnityEngine.Debug.DrawRay(pos + direction, left * arrowHeadLength, color);
        }
    }
}
