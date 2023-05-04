using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;

namespace KWUtils
{
    public struct ChunkClusterComposition
    {
        private FixedList64Bytes<int> topCluster;
        private FixedList64Bytes<int> rightCluster;
        private FixedList64Bytes<int> bottomCluster;
        private FixedList64Bytes<int> leftCluster;
    }

    public struct GateClusterNode
    {
        private int clusterIndex;
        private int chunkIndex;

        // Noeuds auxquel le portail a acces
        private FixedList64Bytes<int> topCluster;
        private FixedList64Bytes<int> rightCluster;
        private FixedList64Bytes<int> bottomCluster;
        private FixedList64Bytes<int> leftCluster;

        public int this[int sideIndex, int index]
        {
            get
            {
                return sideIndex switch
                {
                    0 => topCluster[index],
                    1 => rightCluster[index],
                    2 => bottomCluster[index],
                    3 => leftCluster[index],
                    _ => throw new ArgumentOutOfRangeException(nameof(sideIndex))
                };
            }
        }
    }
    public class PrototypeCLusterNode : MonoBehaviour
    {
        //But: On doit créer PAR CHUNK les Groupements
        // Besoins: portes
        // => on obtiendra les clusters par chunk et par côté sous la forme de List<>
        // Attention, il faut réussir à réunir les pair!
    }
}
