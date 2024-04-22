using System;
using System.ComponentModel;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;

[BurstCompile]
public partial class SetPlayerPositionSystem : SystemBase
{
    public Func<float3> onRequestPlayerPosition;

    [BurstCompile]
    protected override void OnUpdate ()
    {
        Enabled = false;
        return;

        float3 playerPositon = onRequestPlayerPosition.Invoke();

        SetPlayerDataJob setPlayerDataJob = new ()
        {
            playerPosition = playerPositon,
        };

        Dependency = setPlayerDataJob.ScheduleParallel(Dependency);
    }
}

[BurstCompile]
public partial struct SetPlayerDataJob : IJobEntity
{
    [ReadOnly(true)] public float3 playerPosition;

    public void Execute ( MovingRotatingCubeAspect cubeAspect )
    {
        var direction = playerPosition - cubeAspect.localTransform.ValueRO.Position;

        if ( math.dot(direction, direction) < 10.0f )
        {
            direction = -direction;
        }

        cubeAspect.movementVector.ValueRW.movementVector = direction;
    }
}