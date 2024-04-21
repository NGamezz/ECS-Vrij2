using System;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

public class SoulHarvestPillarAuthoring : MonoBehaviour
{
    public float radius;
    public int souls;
    public int desiredSouls;

    private class Baker : Baker<SoulHarvestPillarAuthoring>
    {
        public override void Bake ( SoulHarvestPillarAuthoring authoring )
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);

            AddComponent(entity, new SoulHarvestPillar()
            {
                radius = authoring.radius,
                souls = authoring.souls,
                desiredSouls = authoring.desiredSouls
            });
            AddComponent(entity, new LocalTransform()
            {
                Position = authoring.transform.position,
                Rotation = authoring.transform.rotation,
                Scale = 1.0f,
            });
        }
    }
}

public struct SoulHarvestPillar : IComponentData
{
    public float radius;
    public int souls;
    public int desiredSouls;
}