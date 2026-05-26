using System.Linq;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;

[BurstCompile]
public struct AlignmentData :IComponentData
{
    public float weight;
}

[BurstCompile]
public partial struct AlignmentSystem : ISystem
{
    public void OnUpdate(ref SystemState state)
    {
        var job = new ComputeAlignmentJob();
        job.Schedule();
    }
}

[BurstCompile]
public partial struct ComputeAlignmentJob : IJobEntity
{
    void Execute(in Entity entity, ref FlockingMovementData movementData, DynamicBuffer<NeighbourMovementData> neighbors, in AlignmentData alignmentData)
    {
        movementData.alignment = CalculateAlignment(movementData, neighbors, alignmentData) * alignmentData.weight;
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


