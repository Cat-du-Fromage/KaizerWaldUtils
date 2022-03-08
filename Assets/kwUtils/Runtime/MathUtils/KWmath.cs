using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.Mathematics;
using UnityEngine;
using static Unity.Mathematics.math;
using static Unity.Mathematics.quaternion;

namespace KWUtils
{
    public static class KWmath
    {
        public static bool IsLeft(this Vector2 centerPoint, Vector2 otherPoint) => centerPoint.x < otherPoint.x;
        public static bool IsLeft(this float2 centerPoint, float2 otherPoint) => centerPoint.x < otherPoint.x;
        
        public static bool IsAbove(this Vector2 centerPoint, Vector2 otherPoint) => centerPoint.y > otherPoint.y;
        public static bool IsAbove(this float2 centerPoint, float2 otherPoint) => centerPoint.y > otherPoint.y;

        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float ClampAngle(float lfAngle, float lfMin, float lfMax)
        {
            if (lfAngle < -180f) lfAngle += 360f;
            if (lfAngle > 180) lfAngle -= 360f;
            return Mathf.Clamp(lfAngle, lfMin, lfMax);
        }
        
        //==============================================================================================================
        /// <summary>
        /// Square value : Multiply value by itself (v * v)
        /// </summary>
        //==============================================================================================================
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Sq(int v) => v * v;
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Sq(float v) => v * v;
        
        //==============================================================================================================
        /// <summary>
        /// ComponentWise Multiplication (x * y)
        /// </summary>
        //==============================================================================================================
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int cmul(int2 v) => v.x * v.y;

        //==============================================================================================================
        /// <summary>
        /// ComponentWise substraction (x - y)
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int csub(int2 v) => v.x - v.y;


        // Line Segment Intersection (line1 is p1-p2 and line2 is p3-p4)
        public static bool LineSegmentIntersection(float2 p1, float2 p2, float2 p3, float2 p4, ref float2 result)
        {
            float bx = p2.x - p1.x;
            float by = p2.y - p1.y;
            float dx = p4.x - p3.x;
            float dy = p4.y - p3.y;
            float bDotDPerp = bx * dy - by * dx;
            if (bDotDPerp == 0)
            {
                return false;
            }
            float cx = p3.x - p1.x;
            float cy = p3.y - p1.y;
            float t = (cx * dy - cy * dx) / bDotDPerp;
            if (t is < 0 or > 1)
            {
                return false;
            }
            float u = (cx * by - cy * bx) / bDotDPerp;
            if (u is < 0 or > 1)
            {
                return false;
            }

            result.x = p1.x + t * bx;
            result.y = p1.y + t * by;
            return true;
        }
        
        
        /// <summary>
        /// Return the determinant of 2 vector
        /// </summary>
        /// <param name="v1"></param>
        /// <param name="v2"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Det(in float2 v1, in float2 v2)
        {
            return (v1.x * v2.y) - (v1.y * v2.x);
        }

        /// <summary>
        /// Return the determinant of 2 vector
        /// </summary>
        /// <param name="v1"></param>
        /// <param name="v2"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Det(float v1x, float v1y, float v2x, float v2y)
        {
            return (v1x * v2y) - (v1y * v2x);
        }

        /// <summary>
        /// Return the Determinant of a 3X3 matrix
        /// </summary>
        /// <param name="v1">Lower part of the matrix (g/h/i)</param>
        /// <param name="v2">Mid part of the matrix (d/e/f)</param>
        /// <param name="v3">Top part of the matrix (a/b/c)</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Det(in float3 v1, in float3 v2, in float3 v3)
        {
            return v3.x * Det(float2(v2.y,v1.y), float2(v2.z, v1.z)) - v3.y * Det(float2(v2.x, v1.x), float2(v2.z, v1.z)) + v3.z * Det(float2(v2.x, v1.x), float2(v2.y, v1.y));
        }

        /// <summary>
        /// Get Radius of the circumscribed circle
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="c"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Circumradius(in float2 a, in float2 b, in float2 c)
        {
            float dx = b.x - a.x;
            float dy = b.y - a.y;
            float ex = c.x - a.x;
            float ey = c.y - a.y;
            float bl = dx * dx + dy * dy;
            float cl = ex * ex + ey * ey;
            float d = 0.5f / (dx*ey - dy*ex);
            float x = (ey * bl - dy * cl) * d;
            float y = (dx * cl - ex * bl) * d;
            return x * x + y * y;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float2 GetCircumcenter(in float2 a, in float2 b, in float2 c)
        {
            float dx = b.x - a.x;
            float dy = b.y - a.y;
            //float2 AB = float2(b.x - a.x, b.y - a.y);
            float ex = c.x - a.x;
            float ey = c.y - a.y;
            //float2 AC = float2(c.x - a.x, c.y - a.y);
            float bl = dx * dx + dy * dy;
            float cl = ex * ex + ey * ey;
            float d = 0.5f / (dx * ey - dy * ex);
            float x = a.x + (ey * bl - dy * cl) * d;
            float y = a.y + (dx * cl - ex * bl) * d;
            return float2(x, y);
        }

        #region CircumCenter

        // Function to find the line given two points
        public static float3 LineFromPoint(float2 p1, float2 p2)
        {
            //float a = p2.y - p1.y;
            //float b = p1.x - p2.x;
            //float c = select(a * p1.x + b * p1.y, a * p1.x - b * p1.y, b < 0);
            //float c = a * p1.x + b * p1.y;
            return float3(p2.y - p1.y, p1.x - p2.x, select((p2.y - p1.y) * p1.x + (p1.x - p2.x) * p1.y, (p2.y - p1.y) * p1.x - (p1.x - p2.x) * p1.y, (p1.x - p2.x) < 0));
        }

        public static float3 PerpendicularBisectorFromLine(float2 p1, float2 p2, float3 abc)
        {
            float2 midPoint = float2((p1.x + p2.x) / 2, (p1.y + p2.y) / 2);

            // c = -bx + ay
            /*
            c = -abc.y * (midPoint.x) + abc.x * (midPoint.y);

            float temp = abc.x;
            a = -abc.y;
            b = temp;
            */
            return float3(-abc.y, abc.x, -abc.y * (midPoint.x) + abc.x * (midPoint.y));
        }


        // Returns the intersection point of two lines
        static float2 LineLineIntersection(float2 a, float2 b, float2 c)
        {
            //double determinant = a1 * b2 - a2 * b1;
            /*
            float determinant = Det(a,b);
            if (determinant == 0)
            {
                // The lines are parallel. This is simplified
                // by returning a pair of FLT_MAX
                return float2(Single.MaxValue, Single.MaxValue);
            }
            else
            {
                float x = Det(c,b) / determinant; //(b2 * c1 - b1 * c2) / determinant;
                float y = Det(a, c) / determinant; // (a1 * c2 - a2 * c1) / determinant;
                return float2(x, y);
            }
            */
            return select(float2(Det(c, b) / Det(a, b), Det(a, c) / Det(a, b)), float2(Single.MaxValue, Single.MaxValue), Det(a, b) == 0);
        }


        public static float2 FindCircumCenter(float2 P, float2 Q, float2 R)
        {
            // Line PQ is represented as ax + by = c
            float3 abc = LineFromPoint(P, Q);

            // Line QR is represented as ex + fy = g
            float3 efg = LineFromPoint(Q, R);

            // Converting lines PQ and QR to perpendicular
            // vbisectors. After this, L = ax + by = c
            // M = ex + fy = g
            float3 bisABC = PerpendicularBisectorFromLine(P, Q, LineFromPoint(P, Q));
            float3 bisEFG = PerpendicularBisectorFromLine(Q, R, LineFromPoint(Q, R));

            // The point of intersection of L and M gives
            // the circumcenter

            return LineLineIntersection(float2(bisABC.x, bisEFG.x), float2(bisABC.y, bisEFG.y), float2(bisABC.z, bisEFG.z));
        }
        #endregion CircumCenter
        /// <summary>
        /// return if a point c in left to the vector ab
        /// CAREFUL with direction while assigning a and b : a -> b != b -> a
        /// </summary>
        /// <param name="a">start/origin of the vector ab</param>
        /// <param name="b">end of the vector ab</param>
        /// <param name="c">point checked if positioned left to vector ab</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsLeft(in float2 a, in float2 b, in float2 c)
        {
            return (b.x - a.x) * (c.y - a.y) - (b.y - a.y) * (c.x - a.x) > 0;
        }

        /// <summary>
        /// return if a point c in right to the vector ab
        /// CAREFUL with direction while assigning a and b : a -> b != b -> a
        /// </summary>
        /// <param name="a">start/origin of the vector ab</param>
        /// <param name="b">end of the vector ab</param>
        /// <param name="c">point checked if positioned right to vector ab</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsRight(in float2 a, in float2 b, in float2 c)
        {
            return (b.x - a.x) * (c.y - a.y) - (b.y - a.y) * (c.x - a.x) < 0;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool InCircle(in float2 a, in float2 b, in float2 c, in float2 p)
        {
            float dx = a.x - p.x;
            float dy = a.y - p.y;
            float ex = b.x - p.x;
            float ey = b.y - p.y;
            float fx = c.x - p.x;
            float fy = c.y - p.y;

            float ap = (dx * dx) + (dy * dy);
            float bp = (ex * ex) + (ey * ey);
            float cp = (fx * fx) + (fy * fy);

            return dx * (ey * cp - bp * fy) -
                   dy * (ex * cp - bp * fx) +
                   ap * (ex * fy - ey * fx) < 0;
        }
        
        //From https://github.com/Unity-Technologies/UnityCsReference/blob/master/Runtime/Transform/ScriptBindings/Transform.bindings.cs
        //NOT TESTED YET
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Quaternion RotateFSelf(this Quaternion localRotation, float x, float y, float z)
        {
            Quaternion eulerRot = Quaternion.Euler(x, y, z);
            localRotation *= eulerRot;
            return localRotation;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static quaternion RotateFromSelf(this quaternion localRotation, float3 eulers)
        {
            quaternion eulerRot = EulerZXY(eulers.x, eulers.y, eulers.z);
            localRotation = mul(localRotation,eulerRot);
            return localRotation;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Quaternion RotateFWorld(this Quaternion rotation, float x, float y, float z)
        {
            Quaternion eulerRot = Quaternion.Euler(x, y, z);
            rotation *= (Quaternion.Inverse(rotation) * eulerRot * rotation);
            return rotation;
        }
        
        //NOT TESTED YET
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static quaternion RotateFromWorld(this quaternion rotation, float3 eulers)
        {
            quaternion eulerRot = EulerZXY(eulers.x, eulers.y, eulers.z);
            //rotation = rotation * inverse(rotation) * eulerRot * rotation;
            rotation =mul(rotation, mul(mul(inverse(rotation), eulerRot), rotation));
            return rotation;
        }
        
        
        /*
         EXEMPLES : Careful with conversion with radians!
         Rotate world Space
            quaternion mathCamerarRotation = CameraTransform.rotation;
            quaternion worldRot = mathCamerarRotation.RotateFromWorld(new float3(0f, radians(distanceX * deltaTime),0f));
            CameraTransform.rotation = worldRot;
         Rotate Self Space
            quaternion mathCameraLocalRotation = CameraTransform.localRotation;//need to be reset after first rotation
            quaternion selfRot = mathCameraLocalRotation.RotateFromSelf(new float3(radians(-distanceY * deltaTime), 0f, 0f));
            CameraTransform.localRotation =  selfRot;
         
         */
         //VECTOR
         
         
         // Source: https://stackoverflow.com/questions/3154454/what-is-the-most-efficient-way-to-calculate-the-least-common-multiple-of-two-int
         
         /// <summary>
         /// Greater Common Divisor / Plus grand diviseur commun
         /// </summary>
         public static int gcd(int a, int b)
         {
             if (b == 0)
                 return a;
             return gcd(b, a % b);
             /*
             while (a != 0 && b != 0)
             {
                 if (a > b)
                     a %= b;
                 else
                     b %= a;
             }

             return a | b;
             */
         }

         /// <summary>
         /// Lower Common Multiplicator / Plus petit multiple commun
         /// </summary>
         public static int lcm(int a, int b)
         {
             //return select((b / gcd(a, b)) * a, (a / gcd(a, b)) * b, a > b);
             return a > b ? (a / gcd(a, b)) * b : (b / gcd(a, b)) * a;
         }
    }
}
