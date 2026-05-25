using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;

public struct SeparationData :IComponentData
{
    public float separationRadius;
    public float separationRadiusSqr;
}

[BurstCompile]
public partial struct SeparationSystem : ISystem
{
    public void OnUpdate(ref SystemState state)
    {
        foreach (var (movementData, separationData) in SystemAPI.Query<RefRW<FlockingMovementData>, RefRW<SeparationData>>())
        {
            DynamicBuffer<NeighbourMovementData> buffer = state.EntityManager.GetBuffer<NeighbourMovementData>(movementData.ValueRO.entity);
            float3 separation = CalculateSeparation(movementData.ValueRO, buffer, separationData.ValueRO);
            movementData.ValueRW.velocity += separation;
        }
    }

    public float3 CalculateSeparation(FlockingMovementData agent, DynamicBuffer<NeighbourMovementData> neighbors, SeparationData separationData)
    {
        float3 separation = float3.zero;
        float separationRadius = separationData.separationRadius;

        if (neighbors.Length > 0)
        {
            int closeNeighbors = 0;
            foreach (NeighbourMovementData neighbor in neighbors)
            {
                float3 difference = agent.position - neighbor.position;
                if (math.lengthsq(difference) > separationRadius * separationRadius) continue;
                ++closeNeighbors;
                separation += difference;
            }

            if (closeNeighbors > 0)
            {
                separation /= closeNeighbors;
                if (math.lengthsq(separation) > 1)
                {
                    separation = math.normalizesafe(separation);
                }
            }
        }

        return separation;
    }
}
