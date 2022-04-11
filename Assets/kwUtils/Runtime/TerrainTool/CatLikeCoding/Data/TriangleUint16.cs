using System.Runtime.InteropServices;
using Unity.Mathematics;

namespace KWUtils
{
    [StructLayout(LayoutKind.Sequential)]
    public struct TriangleUInt16 
    {
        public ushort a;
        public ushort b;
        public ushort c;

        public static implicit operator TriangleUInt16 (in int3 t) => new TriangleUInt16 
        {
            a = (ushort)t.x,
            b = (ushort)t.y,
            c = (ushort)t.z
        };
    }
}
