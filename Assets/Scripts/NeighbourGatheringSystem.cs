using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;
using UnityEngine;

public partial struct NeighbourGatheringSystem : ISystem
{
    public void OnUpdate(ref SystemState state)
    {
        var job = new FlockingJob
        {
            deltaTime = SystemAPI.Time.DeltaTime,
            hashGrid = SystemAPI.GetSingleton<HashGridSingleton>(),
            flockingMovementLookup = SystemAPI.GetComponentLookup<FlockingMovementData>(true)
        };

        job.ScheduleParallel(state.Dependency).Complete();
    }

    [BurstCompile]
    public partial struct FlockingJob : IJobEntity
    {
        [ReadOnly]
        public float deltaTime;
        [ReadOnly]
        public HashGridSingleton hashGrid;
        [ReadOnly]
        public ComponentLookup<FlockingMovementData> flockingMovementLookup;

        void Execute(
            in FlockingMovementData movementData,
            ref DynamicBuffer<NeighbourMovementData> neighbours)
        {
            neighbours.Clear();
            int currentCellHash = HashGridUtils.CalculateCellHash(movementData.position, hashGrid.cellSize);
            if (hashGrid.grid.TryGetFirstValue(currentCellHash, out Entity other, out var iterator))
            {
                do
                {
                    if (!flockingMovementLookup.HasComponent(other)) continue;
                    var otherData = flockingMovementLookup[other];
                    if (otherData.id == movementData.id) continue;
                    neighbours.Add(new NeighbourMovementData(otherData));
                    if(neighbours.Capacity >= neighbours.Length) break;
                }
                while (hashGrid.grid.TryGetNextValue(
                    out other,
                    ref iterator));
            }
        }
    }
}
