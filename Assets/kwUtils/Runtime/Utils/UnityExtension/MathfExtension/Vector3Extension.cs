using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.Mathematics;
using UnityEngine;

using static Unity.Mathematics.math;

namespace KWUtils
{
    public static class Vector3Extension
    {
        /// <summary>
        /// flatten the coordinate by setting y to 0
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 Flat(this Vector3 coordToFlat)
        {
            return new Vector3(coordToFlat.x, 0, coordToFlat.z);
        }
        
        /// <summary>
        /// Set Axis to a given destination using XZ axis only
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 FlatMove(this Vector3 coordToMove, Vector3 newPosition)
        {
            return new Vector3(newPosition.x, coordToMove.y, newPosition.z);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 SetX(this Vector3 target, float value)
        {
            return new Vector3(value,target.y,target.z);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 SetY(this Vector3 target, float value)
        {
            return new Vector3(target.x,value,target.z);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 SetZ(this Vector3 target, float value)
        {
            return new Vector3(target.x,target.y,value);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 SetAxis(this Vector3 target, Axis axis, float val)
        {
            Vector3 newPos = Vector3.zero;
            switch (axis)
            {
                case Axis.X:
                    newPos = new Vector3(val, target.y, target.z);
                    break;
                case Axis.Y:
                    newPos = new Vector3(target.x, val, target.z);
                    break;
                case Axis.Z:
                    newPos = new Vector3(target.x, target.y, val);
                    break;
            }
            return newPos;
        }
        
        /// <summary>
        /// Return normalize direction
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 DirectionTo(this Vector3 source, Vector3 destination)
        {
            return Vector3.Normalize(destination - source);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float3 DirectionTo(this float3 source, float3 destination)
        {
            return normalize(destination - source);
        }
        
        /// <summary>
        /// Return distance from 2 vector
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float DistanceTo(this Vector3 source, Vector3 destination)
        {
            return Vector3.Magnitude(destination - source);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2 xy(this Vector3 source)
        {
            return new Vector2(source.x, source.y);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2 xz(this Vector3 source)
        {
            return new Vector2(source.x, source.z);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float2 XY(this Vector3 source)
        {
            return new float2(source.x, source.y);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float2 XZ(this Vector3 source)
        {
            return new float2(source.x, source.z);
        }
    }
}
