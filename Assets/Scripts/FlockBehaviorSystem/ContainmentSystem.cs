using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;

public struct ContainmentData : IComponentData
{
    public float weight;
    public float containmentRadius;
    public float containmentRadiusSqr;
}

[BurstCompile]
public partial struct ContaimentSystem : ISystem
{
    public void OnUpdate(ref SystemState state)
    {
        foreach (var (movementData, containmentData) in SystemAPI.Query<RefRW<FlockingMovementData>, RefRW<ContainmentData>>())
        {
            float3 containment = CalculateContainment(movementData.ValueRO, containmentData.ValueRO) * containmentData.ValueRO.weight;
            movementData.ValueRW.velocity += containment * SystemAPI.Time.DeltaTime;
        }
    }

    public float3 CalculateContainment(FlockingMovementData agent, ContainmentData contaimentData)
    {
        float3 containment = (math.lengthsq(agent.position) > contaimentData.containmentRadiusSqr) ? math.normalize(-agent.position) : float3.zero;
        //float3 containment = math.normalize(-agent.position);
        return containment;
    }
}
