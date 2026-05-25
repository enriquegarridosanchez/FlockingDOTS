using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
using NUnit.Framework;
using System.Collections.Generic;

class SpawnerAuthoring : MonoBehaviour
{
    public GameObject Prefab;
    public float SpawnRate;
    public float SpawnRadius;
    public int MaxSpawnCount;
    public List<BehaviorPresetConfig> behaviors;
}

class SpawnerBaker : Baker<SpawnerAuthoring>
{
    public override void Bake(SpawnerAuthoring authoring)
    {
        var entity = GetEntity(TransformUsageFlags.None);
        AddComponent(entity, new SpawnerData
        {
            Prefab = GetEntity(authoring.Prefab, TransformUsageFlags.Dynamic),
            SpawnPosition = (Vector2)authoring.transform.position,
            SpawnRate = authoring.SpawnRate,
            SpawnRadius = authoring.SpawnRadius,
            MaxSpawnCount = authoring.MaxSpawnCount,
            NextSpawnTime = 0f
        });
    }
}

public struct SpawnerData : IComponentData
{
    public Entity Prefab;
    public float2 SpawnPosition;
    public float SpawnRate;
    public float SpawnRadius;
    public int MaxSpawnCount;
    // This field is used only for the multi-threading example.
    public float NextSpawnTime;
}