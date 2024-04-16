using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public class GenerationConfigAuthoring : MonoBehaviour
{
    public GameObject wallPrefab;
    public GameObject floorPrefab;
    public float2 floorSize;
    public int amountOfFloors;

    private class Baker : Baker<GenerationConfigAuthoring>
    {
        public override void Bake ( GenerationConfigAuthoring authoring )
        {
            Entity entity = GetEntity(TransformUsageFlags.None);

            Entity wallPrefab = GetEntity(authoring.wallPrefab, TransformUsageFlags.None);
            Entity floorPrefab = GetEntity(authoring.floorPrefab, TransformUsageFlags.None);

            AddComponent(entity, new GenerationConfig()
            {
                wallPrefab = wallPrefab,
                floorPrefab = floorPrefab,
                floorSize = authoring.floorSize,
                amountOfFloors = authoring.amountOfFloors,
            });
        }
    }
}

public struct GenerationConfig : IComponentData
{
    public Entity wallPrefab;
    public Entity floorPrefab;
    public float2 floorSize;
    public int amountOfFloors;
}