using System.Collections;
using System.Collections.Generic;
using KWUtils.KWGenericGrid;
using Unity.Mathematics;
using UnityEngine;

public class GridHandlerTwo : MonoBehaviour, IGridHandler<Vector3, GenericChunkedGrid<Vector3>>
{
    public GenericChunkedGrid<Vector3> Grid { get; private set; }
    void Start()
    {
        
    }
    
    public void InitializeGrid(int2 terrainBounds)
    {
        Grid = new GenericChunkedGrid<Vector3>(terrainBounds, 2, 1);
        Grid.UpdateChunk();
        Debug.Log($"ChunkGrid Recieved {terrainBounds}");
    }
}
