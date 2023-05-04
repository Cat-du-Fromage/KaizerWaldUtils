using System;
using Unity.Collections;
using Unity.Mathematics;

namespace KWUtils
{
    public enum ECardinal : int
    {
        Top    = 0,
        Right  = 1,
        Bottom = 2,
        Left   = 3
    }
    
    public struct PairStartLength
    {
        private int2 pair;

        public PairStartLength(int startIndex, int length)
        {
            pair = new int2(startIndex, length);
        }

        public int StartIndex => pair.x;
        public int Length => pair.y;
    }
    public struct ChunkSubCluster
    {
        public FixedList512Bytes<PairStartLength> topCluster;
        public FixedList512Bytes<PairStartLength> rightCluster;
        public FixedList512Bytes<PairStartLength> bottomCluster;
        public FixedList512Bytes<PairStartLength> leftCluster;
        
        public ChunkSubCluster(ChunkComponent clustersInChunk)
        {
            topCluster    = new FixedList512Bytes<PairStartLength>();
            rightCluster  = new FixedList512Bytes<PairStartLength>();
            bottomCluster = new FixedList512Bytes<PairStartLength>();
            leftCluster   = new FixedList512Bytes<PairStartLength>();

            GetSubCluster(ECardinal.Top, clustersInChunk.TopGates);
            GetSubCluster(ECardinal.Right, clustersInChunk.RightGates);
            GetSubCluster(ECardinal.Bottom, clustersInChunk.BottomGates);
            GetSubCluster(ECardinal.Left, clustersInChunk.LeftGates);
        }

        public FixedList512Bytes<PairStartLength> this[int sideIndex]
        {
            get
            {
                return (ECardinal)sideIndex switch
                {
                    ECardinal.Top    => topCluster,
                    ECardinal.Right  => rightCluster,
                    ECardinal.Bottom => bottomCluster,
                    ECardinal.Left   => leftCluster,
                };
            }
        }

        private void GetSubCluster(ECardinal side, Memory<GateCluster> cluster)
        {
            if (cluster.IsEmpty || cluster.Span[0].IsClosed) return;
            Span<Gate> gateSlice = cluster.Span[0].GateSlice.Span;
            int startIndex = 0;
            for (int i = 0; i < gateSlice.Length; i++)
            {
                if (!gateSlice[i].IsClosed) continue;
                if (i!=0)
                {
                    this[(int)side].Add(new PairStartLength(startIndex, i-startIndex));
                }
                startIndex = i + 1;
            }
        }
        
    }
}