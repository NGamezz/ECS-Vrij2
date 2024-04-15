using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public class MovementAuthoring : MonoBehaviour
{
    private class Baker : Baker<MovementAuthoring>
    {
        public override void Bake ( MovementAuthoring authoring )
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);

            AddComponent(entity, new MovementVector()
            {
                movementVector = new float3(UnityEngine.Random.Range(-1, 2), 0, UnityEngine.Random.Range(-1, 2))
            });
        }
    }
}

public struct MovementVector : IComponentData
{
    public float3 movementVector;
}