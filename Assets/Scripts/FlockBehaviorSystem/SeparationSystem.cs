using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;

public struct SeparationData :IComponentData
{
    public float weight;
    public float separationRadius;
    public float separationRadiusSqr;
}

[BurstCompile]
public partial struct SeparationSystem : ISystem
{
    public void OnUpdate(ref SystemState state)
    {
        var job = new ComputeSeparationJob();
        job.Schedule();
    }
}


[BurstCompile]
public partial struct ComputeSeparationJob : IJobEntity
{
    void Execute(in Entity entity, ref FlockingMovementData movementData, DynamicBuffer<NeighbourMovementData> neighbors, in SeparationData separationData)
    {
        movementData.separation = CalculateSeparation(movementData, neighbors, separationData) * separationData.weight;
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