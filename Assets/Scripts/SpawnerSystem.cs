using Unity.Entities;
using Unity.Transforms;
using Unity.Burst;
using Unity.Mathematics;

public partial struct SpawnerSystem : ISystem
{
    public int spawnedCount;
    private float nextSpawn;

    // The Random struct is from the Unity Mathematics package, which provides types
    // and functions optimized for Burst.
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
                velocity = new float3(random.NextFloat2() * 2 - 1, 0)
                //velocity = random.NextFloat3() * 2 - 1, //3D movement
            });

            state.EntityManager.AddBuffer<NeighbourMovementData>(newEntity);

            //// CohesionData
            //state.EntityManager.AddComponent<CohesionData>(newEntity);

            ////// SeparationData
            //state.EntityManager.AddComponent<SeparationData>(newEntity);
            //state.EntityManager.SetComponentData(newEntity, new SeparationData
            //{
            //    separationRadius = 5f
            //});


            nextSpawn = (float)SystemAPI.Time.ElapsedTime + spawner.SpawnRate;

            ++spawnedCount;
        }
    }
}
