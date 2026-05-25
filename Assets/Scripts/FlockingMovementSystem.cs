using System.Collections.Generic;
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

    public void OnUpdate(ref SystemState state)
    {
        foreach (var (movementData, localTransform) in SystemAPI.Query<RefRW<FlockingMovementData>, RefRW<LocalTransform>>())
        {
            DynamicBuffer<NeighbourMovementData> buffer = state.EntityManager.GetBuffer<NeighbourMovementData>(movementData.ValueRO.entity);    

            GetNeighboursInRadius(ref state,movementData.ValueRO, 2f, ref buffer);

            //float3 cohesion     = CalculateCohesion(movementData.ValueRO, buffer);
            //float3 separation   = CalculateSeparation(movementData.ValueRO, buffer);
            //float3 alignment    = CalculateAlignment(movementData.ValueRO, buffer);
            //float3 containment  = 10f * CalculateContainment(movementData.ValueRO, buffer);

            //float3 finalForce = cohesion + separation + alignment + containment;

            //velocity += finalForce * SystemAPI.Time.DeltaTime;

            //if (math.lengthsq(velocity) > 100f)
            //{
            //    velocity = math.normalizesafe(velocity) * 10f;
            //}

            ////APPLY MOVEMENT

            //position += velocity * SystemAPI.Time.DeltaTime;
            //localTransform.ValueRW.Position = position;

            //movementData.ValueRW.velocity = velocity;
            //movementData.ValueRW.position = position;

            float3 velocity = movementData.ValueRO.velocity;
            float3 position = movementData.ValueRO.position;

            position += velocity * SystemAPI.Time.DeltaTime;
            localTransform.ValueRW.Position = position;
            movementData.ValueRW.position = position;
            if (math.lengthsq(velocity) > 0f)
            {
                localTransform.ValueRW.Rotation = quaternion.LookRotationSafe(velocity, math.forward());
            }
        }
    }

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

    //public float3 CalculateCohesion(FlockingMovementData agent, List<FlockingMovementData> neighbors)
    //{
    //    float3 cohesion = float3.zero;
    //    float3 cohesionPos = agent.position;
 
    //    if (neighbors.Count > 0)
    //    {
    //        foreach (FlockingMovementData neighbour in neighbors)
    //        {
    //            cohesionPos += neighbour.position;
    //        }
    //        cohesionPos /= neighbors.Count + 1;

    //        cohesion = cohesionPos - agent.position;
    //        if (math.lengthsq(cohesion) > 1)
    //        {
    //            cohesion = math.normalizesafe(cohesion);
    //        }
    //    }

    //    return cohesion;
    //}

    //public float3 CalculateSeparation(FlockingMovementData agent, List<FlockingMovementData> neighbors)
    //{
    //    float3 separation = float3.zero;
    //    float separationRadius = 3f;
    //    if (neighbors.Count > 0)
    //    {
    //        int closeNeighbors = 0;
    //        foreach (FlockingMovementData neighbour in neighbors)
    //        {
    //            float3 difference = agent.position - neighbour.position;
    //            if (math.lengthsq(difference) > separationRadius * separationRadius) continue;
    //            ++closeNeighbors;
    //            separation += difference;
    //        }
    //        if (closeNeighbors > 0)
    //        {
    //            separation /= closeNeighbors;
    //            if (math.lengthsq(separation) > 1)
    //            {
    //                separation = math.normalizesafe(separation);
    //            }
    //        }
    //    }
    //    return separation;
    //}

    public float3 CalculateAlignment(FlockingMovementData agent, List<FlockingMovementData> neighbors)
    {
        float3 alignment = agent.velocity;
        
        if (neighbors.Count > 0)
        {
            foreach (FlockingMovementData neighbour in neighbors)
            {
                alignment += neighbour.velocity;
            }
        }

        alignment = math.normalizesafe(alignment);

        return alignment;
    }

    public float3 CalculateContainment(FlockingMovementData agent, List<FlockingMovementData> neighbors)
    {
        float3 containment = (math.lengthsq(agent.position)>100*100) ? math.normalize(-agent.position) : float3.zero;

        return containment;
    }
}