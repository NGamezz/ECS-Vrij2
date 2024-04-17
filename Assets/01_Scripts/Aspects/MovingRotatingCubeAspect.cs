using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

public readonly partial struct MovingRotatingCubeAspect : IAspect
{
    public readonly RefRW<RotateSpeed> rotateSpeed;

    public float RotateSpeed
    {
        get => rotateSpeed.ValueRO.value;
        set => rotateSpeed.ValueRW.value = value;
    }

    public readonly RefRW<LocalTransform> localTransform;

    public LocalTransform LocalTransform
    {
        get => localTransform.ValueRO;
        set => localTransform.ValueRW = value;
    }

    public float3 Position
    {
        get => localTransform.ValueRO.Position;
        set => localTransform.ValueRW.Position = value;
    }

    public readonly RefRW<MovementVector> movementVector;

    public float3 MovementVector
    {
        get => movementVector.ValueRO.movementVector;
        set => movementVector.ValueRW.movementVector = value;
    }
}