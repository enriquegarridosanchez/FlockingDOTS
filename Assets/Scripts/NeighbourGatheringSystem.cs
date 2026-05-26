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
        var physicsWorld = SystemAPI.GetSingleton<PhysicsWorldSingleton>();

        var job = new FlockingJob
        {
            deltaTime = SystemAPI.Time.DeltaTime,
            collisionWorld = physicsWorld.CollisionWorld,
            physicsWorld = physicsWorld.PhysicsWorld,
            hashGrid = SystemAPI.GetSingleton<HashGridSingleton>(),
            flockingMovementLookup = SystemAPI.GetComponentLookup<FlockingMovementData>(true)
        };

        state.Dependency = job.ScheduleParallel(state.Dependency);
    }

    [BurstCompile]
    public partial struct FlockingJob : IJobEntity
    {
        [ReadOnly]
        public float deltaTime;
        [ReadOnly]
        public HashGridSingleton hashGrid;
        [ReadOnly]
        public CollisionWorld collisionWorld;
        [ReadOnly]
        public PhysicsWorld physicsWorld;
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
                }
                while (hashGrid.grid.TryGetNextValue(
                    out other,
                    ref iterator));
            }

            // Physics -based neighbor detection (commented out for performance reasons, as it can be expensive to perform distance checks for every agent every frame)
            //NativeList<DistanceHit> hits =
            //    new NativeList<DistanceHit>(Allocator.Temp);

            //var input = new PointDistanceInput
            //{
            //    Position = movementData.position,
            //    MaxDistance = movementData.neighbourRadius,
            //    Filter = CollisionFilter.Default
            //};

            //bool foundAny = collisionWorld.CalculateDistance(input, ref hits);

            //if (foundAny)
            //{
            //    foreach (var hit in hits)
            //    {
            //        Entity other = physicsWorld.Bodies[hit.RigidBodyIndex].Entity;

            //        if (!flockingMovementLookup.HasComponent(other)) continue;
            //        var otherData = flockingMovementLookup[other];
            //        if (otherData.id == movementData.id) continue;
            //        neighbours.Add(new NeighbourMovementData(otherData));
            //    }
            //}

            //hits.Dispose();
        }
    }
}
