using Unity.Entities;
using UnityEngine;

public class RotatingCubeAuthoring : MonoBehaviour
{
    private class Baker : Baker<RotatingCubeAuthoring>
    {
        public override void Bake ( RotatingCubeAuthoring authoring )
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);

            AddComponent(entity, new RotatingCube());
        }
    }
}

public struct RotatingCube : IComponentData
{ }