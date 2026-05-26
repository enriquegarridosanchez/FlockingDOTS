using Unity.Entities;
using Unity.Transforms;
using Unity.Burst;
using Unity.Mathematics;

public partial struct SpawnerSystem : ISystem
{
    public int spawnedCount;
    private float nextSpawn;
    private Random random;

    public void OnCreate(ref SystemState state)
    {
        // This call prevents the system from updating unless at least one entity with
        // the Spawner component exists in the ECS world.
        // This also prevents GetSingleton from throwing an exception if it doesn't find
        // an object of type Spawner.
        state.RequireForUpdate<SpawnerData>();
        random = new Random((uint)System.DateTime.Now.Ticks);
        spawnedCount=0;
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        // Use the GetSingleton method when there is only one entity of a 
        // specific type in the ECS world.
        SpawnerData spawner = SystemAPI.GetSingleton<SpawnerData>();
        if(spawnedCount >= spawner.MaxSpawnCount){ return;}

        if (nextSpawn < SystemAPI.Time.ElapsedTime)
        {
            // The Prefab field of the spawner variable contains a reference to 
            // the entity prefab which ECS converts during the baking stage.
            Entity newEntity = state.EntityManager.Instantiate(spawner.Prefab);
            float2 randomOffset = (random.NextFloat2()*2 - 1) * spawner.SpawnRadius;            
            float3 newPosition = new float3(spawner.SpawnPosition + randomOffset,0f);

            // Local transform
            LocalTransform localTransform = LocalTransform.FromPosition(newPosition);
            state.EntityManager.SetComponentData(newEntity, localTransform);

            // FlockingMovementData
            state.EntityManager.AddComponent<FlockingMovementData>(newEntity);
            state.EntityManager.SetComponentData(newEntity, new FlockingMovementData
            {
                entity = newEntity,
                id = spawnedCount,
                position = newPosition,
                velocity = new float3(math.normalize(random.NextFloat2() * 2f - 1f), 0f),
                maxSpeed = spawner.MaxSpeed,
                maxSpeedSqr = spawner.MaxSpeed * spawner.MaxSpeed,
                neighbourRadius = spawner.NeighbourRadius,
                //velocity = random.NextFloat3() * 2 - 1, //3D movement
            });

            state.EntityManager.AddBuffer<NeighbourMovementData>(newEntity);

            // 1.CohesionData
            state.EntityManager.AddComponent<CohesionData>(newEntity);
            state.EntityManager.SetComponentData(newEntity, new CohesionData
            {
                weight = 50f
            });

            // 2.AvoidanceData
            state.EntityManager.AddComponent<AvoidanceData>(newEntity);
            state.EntityManager.SetComponentData(newEntity, new AvoidanceData
            {
                weight = 80f,
                avoidanceRadius = 30f,
                avoidanceRadiusSqr = 900f
            });

            // 3.SeparationData
            state.EntityManager.AddComponent<SeparationData>(newEntity);
            state.EntityManager.SetComponentData(newEntity, new SeparationData
            {
                weight = 60f,
                separationRadius = 0.5f,
                separationRadiusSqr = 0.25f
            });

            // 4.ContainmentData
            state.EntityManager.AddComponent<ContainmentData>(newEntity);
            state.EntityManager.SetComponentData(newEntity, new ContainmentData
            {
                weight = 80f,
                containmentRadius = 150f,
                containmentRadiusSqr = 150f * 150f
            });

            // 5.AlignmentData
            state.EntityManager.AddComponent<AlignmentData>(newEntity);
            state.EntityManager.SetComponentData(newEntity, new CohesionData
            {
                weight = 40f
            });
            nextSpawn = (float)SystemAPI.Time.ElapsedTime + spawner.SpawnRate;

            ++spawnedCount;
        }
    }
}
