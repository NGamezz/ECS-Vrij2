using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

public readonly partial struct MovingRotatingCubeAspect : IAspect
{
    public readonly RefRO<RotateSpeed> rotateSpeed;
    public readonly RefRW<LocalTransform> localTransform;
    public readonly RefRW<MovementVector> movementVector;
}