using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
using UnityEngine.UI;
using static UnityEngine.GraphicsBuffer;
using Ray = Unity.Physics.Ray;

public struct AvoidanceData : IComponentData
{
    public float3 avoidancePosition;
}

[BurstCompile]
public partial struct AvoidanceSystem : ISystem
{
    public float3 mousePosition;

    public void OnUpdate(ref SystemState state)
    {
        Vector2 mouseScreen = Mouse.current.position.ReadValue();
        Vector3 mouseWorlPos = Camera.main.ScreenToWorldPoint(new Vector3(mouseScreen.x, mouseScreen.y, 0f));

        foreach (var (movementData, avoidanceData) in SystemAPI.Query<RefRW<FlockingMovementData>, RefRW<AvoidanceData>>())
        {
            avoidanceData.ValueRW.avoidancePosition = mouseWorlPos;
            float3 avoidance = CalculateAvoidance(movementData.ValueRO, avoidanceData.ValueRO) * 9999f;
            movementData.ValueRW.velocity += avoidance * SystemAPI.Time.DeltaTime;
        }
    }

    public float3 CalculateAvoidance(FlockingMovementData agent, AvoidanceData avoidanceData)
    {
        float3 avoidance = math.normalizesafe(agent.position - avoidanceData.avoidancePosition);
        return avoidance;
    }
}
