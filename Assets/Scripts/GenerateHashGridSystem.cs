using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using static UnityEngine.EventSystems.EventTrigger;

public struct HashGridSingleton : IComponentData
{
    public float cellSize;
    public NativeParallelMultiHashMap<int, Entity> grid;
}
public partial struct GenerateHashGridSystem : ISystem
{
    private NativeParallelMultiHashMap<int, Entity> grid;
    public void OnCreate(ref SystemState state)
    {
        grid = new NativeParallelMultiHashMap<int, Entity>(10000, Allocator.Persistent);
        Entity singleton = state.EntityManager.CreateEntity();
        state.EntityManager.AddComponentData(singleton,new HashGridSingleton
        {
            cellSize = 5f,
            grid = this.grid
        });
    }

    public void OnDestroy(ref SystemState state)
    {
        if (grid.IsCreated) 
        { 
            grid.Dispose(); 
        }
    }

    public void OnUpdate(ref SystemState state)
    {
        grid.Clear();
        var parallelWriter = grid.AsParallelWriter();
        var job = new GenerateHashGridJob
        {
            hashGrid = SystemAPI.GetSingleton<HashGridSingleton>()
        };

        job.Schedule(state.Dependency).Complete();
    }
}

[BurstCompile]
public partial struct GenerateHashGridJob : IJobEntity
{
    public HashGridSingleton hashGrid;
    void Execute(in Entity entity, in FlockingMovementData movementData)
    {
        int3 cell = HashGridUtils.CalculateCellIndex(movementData.position, hashGrid.cellSize);
        int hash = HashGridUtils.CalculateCellHash(cell);
        hashGrid.grid.Add(hash, entity);
    }
}

public static class HashGridUtils
{
    public static int3 CalculateCellIndex(float3 position, float cellSize)
    {
        return (int3)math.floor(position / cellSize);
    }

    public static int CalculateCellHash(int3 cellIndex)
    {
        return (int)math.hash(cellIndex);
    }
    public static int CalculateCellHash(float3 position, float cellSize)
    {
        return CalculateCellHash(CalculateCellIndex(position, cellSize));
    }
}

