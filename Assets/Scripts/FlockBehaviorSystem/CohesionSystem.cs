using Unity.Entities;
using Unity.Mathematics;

public struct CohesionData :IComponentData
{
    public float cohesionWeight;
}

public partial struct CohesionSystem : ISystem
{
    public void OnUpdate(ref SystemState state)
    {
        foreach (var (movementData, _) in SystemAPI.Query<RefRW<FlockingMovementData>, RefRW<CohesionData>>())
        {
            DynamicBuffer<NeighbourMovementData> buffer = state.EntityManager.GetBuffer<NeighbourMovementData>(movementData.ValueRO.entity);
            float3 cohesion = CalculateCohesion(movementData.ValueRO, buffer);
            movementData.ValueRW.velocity += cohesion * SystemAPI.Time.DeltaTime;
        }
    }

    public float3 CalculateCohesion(in FlockingMovementData agent,in DynamicBuffer<NeighbourMovementData> neighbors)
    {
        float3 cohesion = float3.zero;
        float3 cohesionPos = agent.position;

        if (neighbors.Length > 0)
        {
            foreach (NeighbourMovementData neighbor in neighbors)
            {
                cohesionPos += neighbor.position;
            }
            cohesionPos /= neighbors.Length + 1;

            cohesion = cohesionPos - agent.position;
            if (math.lengthsq(cohesion) > 1)
            {
                cohesion = math.normalizesafe(cohesion);
            }
        }

        return cohesion;
    }
}
