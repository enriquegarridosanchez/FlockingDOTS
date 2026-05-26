using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;

[BurstCompile]
public struct AvoidanceData : IComponentData
{
    public float weight;
    public float avoidanceRadius;
    public float avoidanceRadiusSqr;
}

[BurstCompile]
public partial struct AvoidanceSystem : ISystem
{
    public void OnUpdate(ref SystemState state)
    {
        Vector2 mouseScreen = Mouse.current.position.ReadValue();
        Vector3 mouseWorlPos = Camera.main.ScreenToWorldPoint(new Vector3(mouseScreen.x, mouseScreen.y, 0f));
        mouseWorlPos.z = 0;
        bool isPressedLeft = Mouse.current.leftButton.IsPressed();
        bool isPressedRight = Mouse.current.rightButton.IsPressed();

        var job = new ComputeAvoidanceJob
        {
            activeAvoidance = (isPressedLeft != isPressedRight),
            avoid = isPressedLeft,
            avoidPos = mouseWorlPos
        };
        job.Schedule();
    }
}

[BurstCompile]
public partial struct ComputeAvoidanceJob : IJobEntity
{
    public bool activeAvoidance;
    public bool avoid;
    public float3 avoidPos;
    void Execute(in Entity entity, ref FlockingMovementData movementData, DynamicBuffer<NeighbourMovementData> neighbors, in AvoidanceData avoidanceData)
    {
        movementData.avoidance = CalculateAvoidance(movementData, neighbors, avoidanceData) * avoidanceData.weight;
    }

    public float3 CalculateAvoidance(FlockingMovementData agent, DynamicBuffer<NeighbourMovementData> _, AvoidanceData avoidanceData)
    {
        if(!activeAvoidance){ return float3.zero;}

        float3 avoidance = agent.position - avoidPos;

        if (!avoid)
        {
            avoidance = -avoidance;
        }

        avoidance.z = 0;

        if (math.lengthsq(avoidance) <= avoidanceData.avoidanceRadiusSqr)
        {
            avoidance = math.normalizesafe(avoidance);
        }
        else
        {
            avoidance = float3.zero;
        }

        return avoidance;
    }
}