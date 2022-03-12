using System;
using System.Collections;
using System.Collections.Generic;
using KWUtils;
using KWUtils.KWGenericGrid;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

using static Unity.Jobs.LowLevel.Unsafe.JobsUtility;
using static KWUtils.NativeCollectionExt;

public class FlowField
{
    private readonly int2 gridSize;
    private readonly int chunkSize;
    private readonly int totalNumCells;

    //CostField
    private NativeArray<bool> nativeWalkableCells;
    private NativeArray<byte> nativeCostField;
    //IntegrationField
    private NativeArray<int> nativeBestCostField;
    //FlowField
    private NativeArray<float3> nativeBestDirection;
    
    public byte[] CostField;
    public int[] BestCostField;
    public Vector3[] directionField;

    public FlowField(int2 gridSize, int chunkSize)
    {
        this.gridSize = gridSize;
        this.chunkSize = chunkSize;
        totalNumCells = gridSize.x * gridSize.y;
    }

    public Vector3[] GetFlowField(int targetCell, IGridHandler<bool, GenericGrid<bool>> obstacleGrid)
    {
        return CalculateFlowField(targetCell, obstacleGrid.Grid.GridArray);
    }
    
    public Vector3[] GetFlowField(int targetCell, bool[] walkableCells)
    {
        return CalculateFlowField(targetCell, walkableCells);
    }

    private Vector3[] CalculateFlowField(int targetCell, bool[] walkableCells = null)
    {
        CostField      ??= new byte[totalNumCells];
        BestCostField  ??= new int[totalNumCells];
        directionField ??= new Vector3[totalNumCells];
        
        //Cost Field
        nativeWalkableCells = walkableCells.ToNativeArray();
        nativeCostField = AllocNtvAry<byte>(totalNumCells);
        JobHandle jHCostField = GetCostField();

        //Integration Field
        nativeBestCostField = AllocFillNtvAry<int>(totalNumCells, ushort.MaxValue);
        JobHandle jHIntegrationField = GetIntegrationField(targetCell, jHCostField);
        
        //Direction Field
        nativeBestDirection = AllocNtvAry<float3>(totalNumCells);
        JobHandle jHDirectionField = GetDirectionField(jHIntegrationField);
        jHDirectionField.Complete();
        
        nativeCostField.CopyTo(CostField);
        nativeBestCostField.CopyTo(BestCostField);
        
        //Return value
        nativeBestDirection.Reinterpret<Vector3>().CopyTo(directionField);
        DisposeAll();
        return directionField;
    }

    private void DisposeAll()
    {
        if (nativeWalkableCells.IsCreated) nativeWalkableCells.Dispose();
        if (nativeCostField.IsCreated)     nativeCostField.Dispose();
        if (nativeBestCostField.IsCreated) nativeBestCostField.Dispose();
        if (nativeBestDirection.IsCreated) nativeBestDirection.Dispose();
    }
    
    public JobHandle GetCostField(in JobHandle dependency = default)
    {
        JCostField job = new JCostField
        {
            Obstacles = nativeWalkableCells,
            CostField = nativeCostField,
        };
        return job.ScheduleParallel(totalNumCells, JobWorkerCount - 1, dependency);
    }

    public JobHandle GetIntegrationField(int targetCellIndex, in JobHandle dependency = default)
    {
        JIntegrationField job = new JIntegrationField
        {
            DestinationCellIndex = targetCellIndex,
            MapSizeX = gridSize.x,
            CostField = nativeCostField,
            BestCostField = nativeBestCostField
        };
        return job.Schedule(dependency);
    }

    public JobHandle GetDirectionField(in JobHandle dependency = default)
    {
        JBestDirection job = new JBestDirection
        {
            MapSizeX = gridSize.x,
            BestCostField = nativeBestCostField,
            CellBestDirection = nativeBestDirection
        };
        return job.ScheduleParallel(totalNumCells, JobWorkerCount - 1, dependency);
    }
}

