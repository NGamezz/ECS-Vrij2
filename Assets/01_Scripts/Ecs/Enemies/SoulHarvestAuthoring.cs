using Unity.Entities;
using UnityEngine;

public class SoulHarvestAuthoring : MonoBehaviour
{
    private class Baker : Baker<SoulHarvestAuthoring>
    {
        public override void Bake ( SoulHarvestAuthoring authoring )
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);

            AddComponent(entity, new SoulHarvest());
            SetComponentEnabled<SoulHarvest>(entity, false);
        }
    }
}

public struct SoulHarvest : IComponentData, IEnableableComponent
{
}