using Unity.Burst;
using Unity.Entities;

public partial struct EnemyMoveSystem : ISystem
{
    public void OnCreate ( ref SystemState state )
    {
        state.RequireForUpdate<RotateSpeed>();
    }

    [BurstCompile]
    public void OnUpdate ( ref SystemState state )
    {
        state.Enabled = false;
        return;

        MoveEnemyJob rotateCubesJob = new()
        {
            deltaTime = SystemAPI.Time.DeltaTime,
        };

        state.Dependency = rotateCubesJob.ScheduleParallel(state.Dependency);
    }
}

[BurstCompile, WithAll(typeof(RotatingCube)), WithDisabled(typeof(Stunned))]
public partial struct MoveEnemyJob : IJobEntity
{
    public float deltaTime;

    public void Execute ( MovingRotatingCubeAspect cubeAspect )
    {
        cubeAspect.LocalTransform = cubeAspect.LocalTransform.Translate(cubeAspect.MovementVector * deltaTime);
    }
}