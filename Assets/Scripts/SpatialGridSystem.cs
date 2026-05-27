using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;


// Spatial Grid System: Basic simple implementation of a data structure (MultiHashMap) holding info of every flocking agent
// TO DO: Substitute MultiHashMap by an Octree implementation to avoid avoid spending so much memory


//========================================================================
// Grid Singleton Component Data
//========================================================================

[BurstCompile]
public struct SpatialGridSingleton : IComponentData
{
    public float cellSize;
    public NativeParallelMultiHashMap<int, Entity> grid;
}

//========================================================================
// Generate Grid System
//========================================================================

[BurstCompile]
public partial struct GenerateSpatialGridSystem : ISystem
{
    private NativeParallelMultiHashMap<int, Entity> grid;

    public void OnCreate(ref SystemState state)
    {
        // Created grid as a singleton so it can be reutilized by systems
        grid = new NativeParallelMultiHashMap<int, Entity>(5000, Allocator.Persistent);
        Entity singleton = state.EntityManager.CreateEntity();
        state.EntityManager.AddComponentData(singleton,new SpatialGridSingleton
        {
            // NOTE: Ideally cell size should be the same size as the neighbour radius used in the flocking system
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
        // Empty/Fill the grid every frame
        grid.Clear();
        var flockingMovementQuery = state.GetEntityQuery(ComponentType.ReadOnly<FlockingMovementData>());
        int flockingMovementAmount = flockingMovementQuery.CalculateEntityCount();

        // Grow if too small
        if (grid.Capacity < flockingMovementAmount)
        {
            grid.Capacity = flockingMovementAmount;
        }

        var job = new GenerateSpatialGridJob { gridSingleton = SystemAPI.GetSingleton<SpatialGridSingleton>() };
        
        // TODO: Parallelize this job
        job.Schedule(state.Dependency).Complete();
    }
}

//========================================================================
// Generate Grid Job
//========================================================================

[BurstCompile]
public partial struct GenerateSpatialGridJob : IJobEntity
{
    public SpatialGridSingleton gridSingleton;
    
    // We fill the spatialGrid with the hash of the cell of each entity, and the entiy itself
    void Execute(in Entity entity, in FlockingMovementData movementData)
    {
        int3 cell = SpatialGridUtils.CalculateCellIndex(movementData.position, gridSingleton.cellSize);
        int hash = SpatialGridUtils.CalculateCellHash(cell);
        gridSingleton.grid.Add(hash, entity);
    }
}

//========================================================================
// Utility class: Calculate cell index and hash
//========================================================================

public static class SpatialGridUtils
{

    public static int3 CalculateCellIndex(float3 position, float cellSize)
    {
        return (int3)math.floor(position / cellSize);
    }

    // Hash function from:
    // https://stackoverflow.com/questions/5928725/hashing-2d-3d-and-nd-vectors
    public static int CalculateCellHash(int3 cellIndex)
    {
        return (int)((cellIndex.x * 73856093) ^ (cellIndex.y * 19349663) ^ (cellIndex.z * 83492791));
    }
    public static int CalculateCellHash(float3 position, float cellSize)
    {
        return CalculateCellHash(CalculateCellIndex(position, cellSize));
    }
}

//EOF