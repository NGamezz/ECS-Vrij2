using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;

public partial struct SpawnCubesSystem : ISystem
{
    public void OnCreate ( ref SystemState state )
    {
        state.RequireForUpdate<SpawnCubesConfig>();
    }

    [BurstCompile]
    public void OnUpdate ( ref SystemState state )
    {
        state.Enabled = false;

        SpawnCubesConfig spawnCubesConfig = SystemAPI.GetSingleton<SpawnCubesConfig>();

        NativeArray<Entity> entities = new(spawnCubesConfig.amountToSpawn, Allocator.Temp);

        state.EntityManager.Instantiate(spawnCubesConfig.prefab, entities);

        for ( int i = 0; i < entities.Length; i++ )
        {
            SystemAPI.SetComponent(entities[i], new LocalTransform()
            {
                Position = new Unity.Mathematics.float3(UnityEngine.Random.Range(-5, 6), 0.5f, UnityEngine.Random.Range(-5, 6)),
                Scale = 1.0f,
                Rotation = Unity.Mathematics.quaternion.identity,
            });
        }
    }
}