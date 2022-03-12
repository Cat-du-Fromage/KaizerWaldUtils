using System.Collections;
using System.Collections.Generic;
using KWUtils.KWGenericGrid;
using Unity.Mathematics;
using UnityEngine;

public class ObstacleManager : MonoBehaviour, IGridHandler<bool, GenericGrid<bool>>
{
    public IGridSystem GridSystem { get; set; }
    public GenericGrid<bool> Grid { get; private set; }

    public void InitializeGrid(int2 terrainBounds)
    {
        Grid = new GenericGrid<bool>(terrainBounds, 2);
        Debug.Log($"ObstacleManager Init {Grid.GridArray.Length}");
    }
}
