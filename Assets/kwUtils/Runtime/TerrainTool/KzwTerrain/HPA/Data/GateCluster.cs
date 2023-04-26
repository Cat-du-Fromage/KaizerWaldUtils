using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

using static KWUtils.GateOrientation;
using static Unity.Mathematics.bool4;

namespace KWUtils
{
    public enum GateOrientation : byte
    {
        Horizontal = 0,
        Vertical = 1
    }

    // Pour le moment Représente une ligne entière
    public struct GateCluster
    {
        public bool IsClosed { get; private set; }
        public GateOrientation Orientation;
        public Memory<Gate> GateSlice;
        public Memory<bool4> Side1;
        public Memory<bool4> Side2;

        public GateCluster(int indexCluster, int2 numSpaceHV, int2 numChunkXY, Memory<Gate> gateSlice,
            Memory<bool4> side1, Memory<bool4> side2)
        {
            IsClosed = false;
            GateSlice = gateSlice;
            Orientation = (numChunkXY.x is 1 || numChunkXY.y is 1)
                ? numChunkXY.x is 1 ? Vertical : Horizontal
                : (indexCluster < numSpaceHV.x) ? Horizontal : Vertical;
            Side1 = side1;
            Side2 = side2;
        }

        public readonly int Length => GateSlice.Span.Length;
        public readonly Gate this[int index] => GateSlice.Span[index];

        // Close/Open Function
        public void Toggle() => UpdateGateState(!IsClosed);
        public bool SetClosed() => UpdateGateState(true);
        public bool SetOpen() => UpdateGateState(false);

        private bool UpdateGateState(bool newValue)
        {
            if (IsClosed == newValue) return false;
            IsClosed = newValue;
            Side1.Span[0] = Orientation == GateOrientation.Horizontal
                ? new bool4(Side1.Span[0].x, newValue, Side1.Span[0].zw)
                : new bool4(Side1.Span[0].xy, newValue, Side1.Span[0].w);
            Side2.Span[0] = Orientation == GateOrientation.Horizontal
                ? new bool4(Side2.Span[0].xyz, newValue)
                : new bool4(newValue, Side2.Span[0].yzw);
            return true;
        }
    }

    [BurstCompile]
    public struct JUpdateImmediate : IJob
    {
        [ReadOnly] public GateOrientation Orientation;
        [ReadOnly] public NativeSlice<Gate> Gates;
        public NativeReference<bool> IsClosed;
        public NativeSlice<bool4> ChunkSide1;
        public NativeSlice<bool4> ChunkSide2;

        public void Execute()
        {
            for (int i = 0; i < Gates.Length; i++)
            {
                if (Gates[i].IsClosed) continue;
                // porte enregistrée comme fermée => update les chunks
                UpdateGateState(false);
                return;
            }

            //si la porte est noté ouvert mais après boucle est fermée => update
            UpdateGateState(true);
        }

        private void UpdateGateState(bool newValue)
        {
            if (IsClosed.Value == newValue) return;
            IsClosed.Value = newValue;
            ChunkSide1[0] = Orientation == GateOrientation.Horizontal
                ? new bool4(ChunkSide1[0].x, newValue, ChunkSide1[0].zw)
                : new bool4(ChunkSide1[0].xy, newValue, ChunkSide1[0].w);
            ChunkSide2[0] = Orientation == GateOrientation.Horizontal
                ? new bool4(ChunkSide2[0].xyz, newValue)
                : new bool4(newValue, ChunkSide2[0].yzw);
        }
    }
}