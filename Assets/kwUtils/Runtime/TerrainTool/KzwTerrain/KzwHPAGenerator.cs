using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

using static Unity.Mathematics.math;
using static KWUtils.KWmath;
using static KWUtils.KWGrid;
using static KWUtils.KWChunk;

namespace KWUtils
{
    public class KzwHPAGenerator : MonoBehaviour
    {
        private KzwTerrainGenerator terrain;

        private GenericChunkedGrid<int> test;

        private List<Gate> TerrainGates;

        private void Awake()
        {
            bool terrainExist = TryGetComponent(out terrain);
        }

        private bool IsSideOpen()
        {
            return true;
        }

        private void BuildGates(int numChunks, in GridData gridData)
        {
            
            for (int i = 0; i < numChunks; i++)
            {
                (int x, int y) = GetXY(i, gridData.NumChunkXY.x);
                
                //Vérifier si x/y sont sur les bords
                bool2 xLimit = new bool2(x > 0, x < gridData.NumChunkXY.x - 1);
                bool2 yLimit = new bool2(y > 0, y < gridData.NumChunkXY.y - 1);

            }

            // On va traverser les espaces entre les partitions
            // + = porte
            // [m]+[n]+[o]+[p]
            // [i]+[j]+[k]+[l]
            // [e]+[f]+[g]+[h]
            // [a]+[b]+[c]+[d]
            // ATTENTION !!!
            // porte par ligne = (Nombre Partition sur X) - 1
            // MAIS! la hauteur est INCHANGEE!!! donc bounds: int2(numChunkX-1, numChunkX)
            void GetGatesHorizontal(int numChunkX)
            {
                int width = numChunkX - 1;
                int numIteration = (numChunkX - 1) * numChunkX;
                for (int i = 0; i < numIteration; i++)
                {
                    //coordonnées correspondant ici à l'espace entre 2 partitions
                    (int x, int y) = GetXY(i, width);
                    
                    //Nous Allons utiliser le chunk a gauche
                    //le nombre d'espace par ligne est différent du nombre de chunk par ligne de 1
                    //il faut donc compenser, formule : x += (y * différence Width (ici 1 donc juste y))
                    int2 chunkCoord = int2(x+y, y);
                    
                }
            }
        }
        /*
        private void CreateGates(in GridData gridData)
        {
            const int NumberSide = 4;
            for (int i = 0; i < NumberSide; i++)
            {
                HPASide cardinal = (HPASide)i;
                TerrainGates[i] = new List<Gate>() { InitializeGate(cardinal, gridData) };
                AllGates.AddRange(Gates[i]);
            }
        }
        
        private Gate InitializeGate(HPASide cardinal, in GridData gridData)
        {
            int gateIndex = GetCardinalFromIndex(cardinal, gridData.ChunkSize);
            return new Gate(ChunkIndex, gateIndex, cardinal, gridData);
        }
        
        private int GetCardinalFromIndex(HPASide cardinal, int chunkWidth)
        {
            int topRightCell = Sq(chunkWidth);
            int topLeftCell = topRightCell - chunkWidth - 1;
            
            int middleY = (topLeftCell / chunkWidth) / 2;
            int lastIndexInRow0 = chunkWidth - 1;

            return CardinalFromIndex();
            //=====================================================================
            //Private Methods
            
            int CardinalFromIndex() => cardinal switch
            {
                HPASide.Top    => (topRightCell + topLeftCell) / 2,
                HPASide.Right  => (middleY * chunkWidth) + lastIndexInRow0,
                HPASide.Bottom => lastIndexInRow0 / 2,
                HPASide.Left   => middleY * chunkWidth,
                _ => 0
            };
        }
        */
    }
}
