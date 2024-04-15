using Unity.Burst;
using Unity.Entities;

public partial struct RotatingMovingCubesSystem : ISystem
{
    public void OnCreate ( ref SystemState state )
    {
        state.RequireForUpdate<RotateSpeed>();
    }

    [BurstCompile]
    public void OnUpdate ( ref SystemState state )
    {
        RotateCubesJob rotateCubesJob = new()
        {
            deltaTime = SystemAPI.Time.DeltaTime,
        };

        state.Dependency = rotateCubesJob.ScheduleParallel(state.Dependency);
    }
}

[BurstCompile, WithAll(typeof(RotatingCube))]
public partial struct RotateCubesJob : IJobEntity
{
    public float deltaTime;

    public void Execute ( MovingRotatingCubeAspect movingRotatingCubeAspect )
    {
        movingRotatingCubeAspect.localTransform.ValueRW = movingRotatingCubeAspect.localTransform.ValueRO.RotateY(movingRotatingCubeAspect.rotateSpeed.ValueRO.value * deltaTime);

        movingRotatingCubeAspect.localTransform.ValueRW = movingRotatingCubeAspect.localTransform.ValueRO.Translate(movingRotatingCubeAspect.movementVector.ValueRO.movementVector * deltaTime);
    }
}