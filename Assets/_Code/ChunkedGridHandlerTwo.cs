using System.Collections;
using System.Collections.Generic;
using KWUtils;
using KWUtils.KWGenericGrid;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;

public class ChunkedGridHandlerTwo : MonoBehaviour, IGridHandler<Vector3, GenericChunkedGrid<Vector3>>
{
    public bool EnableDebug;
    public GenericChunkedGrid<Vector3> Grid { get; private set; }

    public void InitializeGrid(int2 terrainBounds)
    {
        Grid = new GenericChunkedGrid<Vector3>(terrainBounds, 16, 1);
        //Debug.Log($"ChunkGrid Recieved {terrainBounds}");
    }
    
    private void OnDrawGizmos()
    {
        if (!EnableDebug || Grid.GridArray.IsNullOrEmpty()) return;
        DebugChunk();
        DebugCell();
    }

    private void DebugChunk()
    {
        Vector3 chunkBounds = new Vector3(16, 0.1f, 16);
        foreach ((int index, _) in Grid.ChunkDictionary)
        {
            Gizmos.color = Color.red;
            Vector3 chunkCenter = Grid.GetChunkCenter(index);
            Gizmos.DrawWireCube(chunkCenter, chunkBounds);
        }
    }

    private void DebugCell()
    {
        GUIStyle style = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter };
        Gizmos.color = Color.green;
        Vector3 bounds = new Vector3(1, 0, 1);
        int iteration = (32 * 32);
        for (int i = 0; i < Grid.GridArray.Length; i++)
        {
            Vector3 centerCell = Grid.GetCellCenter(i);
            //string cellIndex = Grid.GetCellChunkIndexFromGridIndex(i).ToString();
            //Handles.Label(centerCell, cellIndex, style);
            Gizmos.DrawWireCube(centerCell, bounds);
        }
    }
}
