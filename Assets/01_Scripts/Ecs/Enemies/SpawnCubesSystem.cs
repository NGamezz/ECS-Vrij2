using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

[BurstCompile]
public partial class SpawnEnemies : SystemBase
{
    public Func<float3> requestPlayerPosition;

    protected override void OnCreate ()
    {
        RequireForUpdate<SpawnCubesConfig>();
    }

    [BurstCompile]
    protected override void OnUpdate ()
    {
        Enabled = false;
        return;

        EntityCommandBuffer buffer = new(Allocator.TempJob);
        foreach ( var (spawnTimer, config) in SystemAPI.Query<RefRW<SpawnTimer>, RefRO<SpawnCubesConfig>>() )
        {
            spawnTimer.ValueRW.Value = spawnTimer.ValueRO.Value - SystemAPI.Time.DeltaTime;
            if ( spawnTimer.ValueRO.Value > 0 )
            { continue; }

            spawnTimer.ValueRW.Value = config.ValueRO.timeToSpawn;

            var seed = (uint)UnityEngine.Random.Range(0, int.MaxValue);
            Unity.Mathematics.Random random = new(seed);

            var playerPos = requestPlayerPosition.Invoke();

            SpawnJob spawnEnemiesJob = new()
            {
                playerPos = playerPos,
                Ecb = buffer.AsParallelWriter(),
                config = config,
                random = random,
            };

            spawnEnemiesJob.Schedule(config.ValueRO.amountToSpawn, 64).Complete();
        }
        buffer.Playback(EntityManager);
        buffer.Dispose();
    }
}

[BurstCompile]
public struct SpawnJob : IJobParallelFor
{
    public EntityCommandBuffer.ParallelWriter Ecb;

    [ReadOnly] public Unity.Mathematics.Random random;

    [ReadOnly] public float3 playerPos;

    [ReadOnly, NativeDisableUnsafePtrRestriction] public RefRO<SpawnCubesConfig> config;

    [BurstCompile]
    public void Execute ( int index )
    {
        float3 position = playerPos + new float3(random.NextFloat3((int)config.ValueRO.spawnBounds.x, (int)config.ValueRO.spawnBounds.y));

        position.y = 1.0f;

        var entity = Ecb.Instantiate(index, config.ValueRO.prefab);
        Ecb.SetComponent(index, entity, new LocalTransform()
        {
            Scale = 1.0f,
            Rotation = quaternion.identity,
            Position = position
        });
    }
}