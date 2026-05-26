using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using static UnityEngine.EventSystems.EventTrigger;

[BurstCompile]
public struct CohesionData :IComponentData
{
    public float weight;
}

[BurstCompile]
public partial struct CohesionSystem : ISystem
{
    public void OnUpdate(ref SystemState state)
    {
        var job = new ComputeCohesionJob();
        job.Schedule();
    }
}

[BurstCompile]
public partial struct ComputeCohesionJob : IJobEntity
{
    public float deltaTime;
    void Execute(in Entity entity, ref FlockingMovementData movementData, DynamicBuffer<NeighbourMovementData> neighbors, in CohesionData cohesionData)
    {
        movementData.cohesion = CalculateCohesion(movementData, neighbors, cohesionData) * cohesionData.weight;
    }

    public float3 CalculateCohesion(in FlockingMovementData agent, in DynamicBuffer<NeighbourMovementData> neighbors, in CohesionData _)
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
