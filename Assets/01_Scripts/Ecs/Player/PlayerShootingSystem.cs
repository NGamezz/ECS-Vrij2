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
        playerPos.y -= 0.5f;

        var playerRotation = requestPlayerRotation.Invoke();

        SpawnCubesConfig config = SystemAPI.GetSingleton<SpawnCubesConfig>();

        EntityCommandBuffer buffer = new(Unity.Collections.Allocator.Temp);

        foreach ( var localTransform in SystemAPI.Query<RefRO<LocalTransform>>().WithAll<Player>().WithDisabled<Stunned>() )
        {
            float3 forward = math.mul(playerRotation, new float3(0f, 0f, 1f));
            float3 rayEnd = forward * 50.0f + playerPos;

            RaycastInput rayInput = new()
            {
                Start = playerPos,
                End = rayEnd,
                Filter = CollisionFilter.Default,
            };

            var succes = world.CastRay(rayInput, out Unity.Physics.RaycastHit closestHit);
            Debug.DrawRay(playerPos, rayEnd, Color.cyan, 10.0f);

            if ( !succes )
            {
                continue;
            }
            else if ( EntityManager.IsComponentEnabled<Stunned>(closestHit.Entity) )
            {
                continue;
            }

            buffer.SetComponentEnabled<Stunned>(closestHit.Entity, true);
            buffer.SetComponentEnabled<SoulHarvest>(closestHit.Entity, true);

            OnShoot?.Invoke(closestHit.Position, EventArgs.Empty);
        }

        buffer.Playback(EntityManager);
    }
}