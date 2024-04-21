using Unity.Entities;
using UnityEngine;

public class PillarCompletedAuthoring : MonoBehaviour
{
    private class Baker : Baker<PillarCompletedAuthoring>
    {
        public override void Bake ( PillarCompletedAuthoring authoring )
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new PillarCompleted());
            SetComponentEnabled<PillarCompleted>(entity, false);
        }
    }
}

public struct PillarCompleted : IComponentData, IEnableableComponent
{
}