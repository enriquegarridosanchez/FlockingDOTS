using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;

public struct AvoidanceData : IComponentData
{
    public float weight;
    public float3 avoidancePosition;
    public float avoidanceRadius;
    public float avoidanceRadiusSqr;
}

[BurstCompile]
public partial struct AvoidanceSystem : ISystem
{
    public float3 mousePosition;

    public void OnUpdate(ref SystemState state)
    {
        Vector2 mouseScreen = Mouse.current.position.ReadValue();
        Vector3 mouseWorlPos = Camera.main.ScreenToWorldPoint(new Vector3(mouseScreen.x, mouseScreen.y, 0f));
        mouseWorlPos.z = 0;

        bool isPressedLeft  = Mouse.current.leftButton.IsPressed();
        bool isPressedRight = Mouse.current.rightButton.IsPressed();

        if (isPressedLeft == isPressedRight) return;

        foreach (var (movementData, avoidanceData) in SystemAPI.Query<RefRW<FlockingMovementData>, RefRW<AvoidanceData>>())
        {
            avoidanceData.ValueRW.avoidancePosition = mouseWorlPos;
            float3 avoidance = CalculateAvoidance(movementData.ValueRO, avoidanceData.ValueRO) * avoidanceData.ValueRO.weight;
            movementData.ValueRW.velocity += avoidance * SystemAPI.Time.DeltaTime;
        }
    }

    public float3 CalculateAvoidance(FlockingMovementData agent, AvoidanceData avoidanceData)
    {
        float3 avoidance = agent.position - avoidanceData.avoidancePosition;

        bool isPressedLeft = Mouse.current.leftButton.IsPressed();
        if (!isPressedLeft)
        {
            avoidance = avoidanceData.avoidancePosition - agent.position;
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
