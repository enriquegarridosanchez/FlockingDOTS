using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

[BurstCompile]
public struct ContainmentData : IComponentData
{
    public float weight;
    public float containmentRadius;
    public float containmentRadiusSqr;
}

[BurstCompile]
public partial struct ContainmentSystem : ISystem
{
    public void OnUpdate(ref SystemState state)
    {
        var job = new ComputeContainmentJob();
        job.Schedule();
    }
}

[BurstCompile]
public partial struct ComputeContainmentJob : IJobEntity
{
    void Execute(in Entity entity, ref FlockingMovementData movementData, DynamicBuffer<NeighbourMovementData> neighbors, in ContainmentData containmentData)
    {
        movementData.containment = CalculateContainment(movementData, neighbors, containmentData) * containmentData.weight;
    }

    public float3 CalculateContainment(FlockingMovementData agent, DynamicBuffer<NeighbourMovementData> _, ContainmentData contaimentData)
    {
        float3 containment = (math.lengthsq(agent.position) > contaimentData.containmentRadiusSqr) ? -agent.position : float3.zero;
        return containment;
    }
}