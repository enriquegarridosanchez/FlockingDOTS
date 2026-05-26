using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;

public struct FlockingMovementData : IComponentData
{
    public Entity entity;
    public int id;
    public float3 position;
    public float3 velocity;
    public float neighbourRadius;
    public float maxSpeed;
    public float maxSpeedSqr;

    public float3 cohesion;
    public float3 alignment;
    public float3 separation;
    public float3 avoidance;
    public float3 containment;
}

[InternalBufferCapacity(8)]
public struct NeighbourMovementData : IBufferElementData
{
    public Entity entity;
    public int id;
    public float3 position;
    public float3 velocity;

    public NeighbourMovementData(FlockingMovementData flockingMovementData) : this()
    {
        entity      = flockingMovementData.entity;
        id          = flockingMovementData.id;
        position    = flockingMovementData.position;
        velocity    = flockingMovementData.velocity;
    }
}

public class FlockingMovementAuthoring : MonoBehaviour
{
    public class Baker : Baker<FlockingMovementAuthoring>
    {
        public override void Bake(FlockingMovementAuthoring authoring)
        {
        }
    }
}