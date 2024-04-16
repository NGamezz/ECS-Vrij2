using System;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;
using UnityEngine;

[BurstCompile]
public partial class PlayerShootingSystem : SystemBase
{
    public event Action<object, EventArgs> OnShoot;

    public Func<float3> requestPlayerPosition;
    public Func<Quaternion> requestPlayerRotation;

    protected override void OnCreate ()
    {
        RequireForUpdate<Player>();
    }

    [BurstCompile]
    protected override void OnUpdate ()
    {
        if ( !Input.GetKeyDown(KeyCode.E) )
        { return; }

        PhysicsWorld world = SystemAPI.GetSingletonRW<PhysicsWorldSingleton>().ValueRW.PhysicsWorld;

        float3 playerPos = requestPlayerPosition.Invoke();
        var playerRotation = requestPlayerRotation.Invoke();

        SpawnCubesConfig config = SystemAPI.GetSingleton<SpawnCubesConfig>();

        EntityCommandBuffer buffer = new(Unity.Collections.Allocator.Temp);

        foreach ( var (localTransform, entity) in SystemAPI.Query<RefRO<LocalTransform>>().WithAll<Player>().WithDisabled<Stunned>().WithEntityAccess() )
        {
            float3 forward = math.mul(playerRotation, new float3(0f, 0f, 1f));
            float3 rayEnd = forward * 50.0f + playerPos;

            RaycastInput rayInput = new()
            {
                Start = playerPos,
                End = rayEnd,
                Filter = CollisionFilter.Default,
            };

            world.CastRay(rayInput, out Unity.Physics.RaycastHit closestHit);

            if(entity == null)
            {
                continue;
            }

            SystemAPI.SetComponentEnabled<Stunned>(closestHit.Entity, true);

            OnShoot?.Invoke(entity, EventArgs.Empty);
        }

        buffer.Playback(EntityManager);
    }
}