using System;
using UnityEngine;

namespace KWUtils
{
    [Flags]
    public enum Axis
    {
        X = 1 << 0,
        Y = 1 << 1,
        Z = 1 << 2,
        W = 1 << 3
    }
    
    public static class QuaternionExt
    {
        public static Quaternion SetAxis(this Quaternion target, Axis axis ,float val) 
        => axis switch
        {
            Axis.X => new Quaternion(val, target.y, target.z, target.w),
            Axis.Y => new Quaternion(target.x, val, target.z, target.w),
            Axis.Z => new Quaternion(target.x, target.y, val, target.w),
            Axis.W => new Quaternion(target.x, target.y, target.z, val),
            _ => target
        };
        
        public static Quaternion ClampRotationAroundXAxis(this Quaternion q, float minX, float maxX)
        {
            q.x /= q.w;
            q.y /= q.w;
            q.z /= q.w;
            q.w = 1.0f;
 
            float angleX = 2.0f * Mathf.Rad2Deg * Mathf.Atan(q.x);
            angleX = Mathf.Clamp(angleX, minX, maxX);
            q.x = Mathf.Tan(0.5f * Mathf.Deg2Rad * angleX);
 
            return q;
        }
    }
    
    
}