using System.Linq;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;

public struct AlignmentData :IComponentData
{

}

[BurstCompile]
public partial struct AlignmentSystem : ISystem
{
    public void OnUpdate(ref SystemState state)
    {
        foreach (var (movementData, alignmentData) in SystemAPI.Query<RefRW<FlockingMovementData>, RefRW<AlignmentData>>())
        {
            DynamicBuffer<NeighbourMovementData> buffer = state.EntityManager.GetBuffer<NeighbourMovementData>(movementData.ValueRO.entity);
            float3 alignment = CalculateAlignment(movementData.ValueRO, buffer, alignmentData.ValueRO);
            movementData.ValueRW.velocity += alignment * SystemAPI.Time.DeltaTime; ;
        }
    }

    public float3 CalculateAlignment(FlockingMovementData agent, DynamicBuffer<NeighbourMovementData> neighbors, AlignmentData alignmentData)
    {
        float3 alignment = agent.velocity;

        if (neighbors.Length > 0)
        {
            foreach (NeighbourMovementData neighbor in neighbors)
            {
                alignment += neighbor.velocity;
            }
        }

        alignment = math.normalizesafe(alignment);

        return alignment;
    }
}
