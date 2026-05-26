using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

[BurstCompile]
public partial struct FlockingMovementSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<FlockingMovementData>();
    }
    public void OnUpdate(ref SystemState state)
    {
        var job = new FlockingMovementJob
        {
            deltaTime = SystemAPI.Time.DeltaTime
        };
        job.Schedule();
    }
}

[BurstCompile]
public partial struct FlockingMovementJob : IJobEntity
{
    public float deltaTime;

    void Execute(in Entity entity, ref FlockingMovementData movementData, ref LocalTransform localTransform)
    {
        float3 appliedForce = float3.zero;
        appliedForce += movementData.cohesion;
        appliedForce += movementData.separation;
        appliedForce += movementData.containment;
        appliedForce += movementData.alignment;
        appliedForce += movementData.avoidance;

        float3 newVelocity = movementData.velocity + appliedForce * deltaTime;

        if (math.lengthsq(newVelocity) > movementData.maxSpeedSqr)
        {
            newVelocity = math.normalizesafe(newVelocity) * movementData.maxSpeed;
        }

        movementData.velocity = newVelocity;

        if (math.lengthsq(newVelocity) > 0f)
        {
            movementData.position += movementData.velocity * deltaTime;
            localTransform.Position = movementData.position;
            localTransform.Rotation = quaternion.LookRotationSafe(movementData.velocity, math.forward());
        }
    }
}