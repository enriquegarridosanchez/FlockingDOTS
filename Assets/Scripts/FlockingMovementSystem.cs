using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;
using UnityEngine;

public partial struct FlockingMovementSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<FlockingMovementData>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        foreach (var (movementData, localTransform) in SystemAPI.Query<RefRW<FlockingMovementData>, RefRW<LocalTransform>>())
        {
            float3 appliedForce = float3.zero;
            appliedForce += movementData.ValueRO.cohesion;
            appliedForce += movementData.ValueRO.separation;
            appliedForce += movementData.ValueRO.containment;
            appliedForce += movementData.ValueRO.alignment;
            appliedForce += movementData.ValueRO.avoidance;

            float3 newVelocity = movementData.ValueRO.velocity + appliedForce * SystemAPI.Time.DeltaTime;

            if (math.lengthsq(newVelocity) > movementData.ValueRO.maxSpeedSqr)
            {
                newVelocity = math.normalizesafe(newVelocity) * movementData.ValueRO.maxSpeed;
            }

            float3 position = movementData.ValueRO.position;

            position += newVelocity * SystemAPI.Time.DeltaTime;
            localTransform.ValueRW.Position = position;
            movementData.ValueRW.position = position;
            movementData.ValueRW.velocity = newVelocity;
            if (math.lengthsq(newVelocity) > 0f)
            {
                localTransform.ValueRW.Rotation = quaternion.LookRotationSafe(newVelocity, math.forward());
            }
        }
    }

    [BurstCompile]
    void GetNeighboursInRadius(ref SystemState state, in FlockingMovementData agent, in float radius, ref DynamicBuffer<NeighbourMovementData> neighbors)
    {
        neighbors.Clear();

        float3 position = agent.position;

        var physicsWorldSingleton = SystemAPI.GetSingleton<PhysicsWorldSingleton>();
        CollisionWorld collisionWorld = physicsWorldSingleton.CollisionWorld;
        NativeList<DistanceHit> hits = new NativeList<DistanceHit>(Allocator.Temp);

        var input = new PointDistanceInput
        {
            Position = position,
            MaxDistance = radius,
            Filter = CollisionFilter.Default
        };

        bool foundAny = collisionWorld.CalculateDistance(input, ref hits);

        if (foundAny)
        {
            foreach (var hit in hits)
            {
                Entity hittedEntity = physicsWorldSingleton.PhysicsWorld.Bodies[hit.RigidBodyIndex].Entity;
                FlockingMovementData hittedFlockingData = state.EntityManager.GetComponentData<FlockingMovementData>(hittedEntity);
                if (hittedFlockingData.id == agent.id) continue;
                neighbors.Add(new NeighbourMovementData(hittedFlockingData));
            }
        }
    }
}