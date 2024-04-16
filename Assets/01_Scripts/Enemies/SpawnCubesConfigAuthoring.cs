using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public class SpawnCubesConfigAuthoring : MonoBehaviour
{
    public GameObject prefab;
    public int amountToSpawn;

    public float3 spawnBounds;

    private class Baker : Baker<SpawnCubesConfigAuthoring>
    {
        public override void Bake ( SpawnCubesConfigAuthoring authoring )
        {
            Entity entity = GetEntity(TransformUsageFlags.None);

            AddComponent(entity, new SpawnCubesConfig()
            {
                prefab = GetEntity(authoring.prefab, TransformUsageFlags.Dynamic),
                amountToSpawn = authoring.amountToSpawn,
                spawnBounds = authoring.spawnBounds,
            });
        }
    }
}

public struct SpawnCubesConfig : IComponentData
{
    public Entity prefab;
    public int amountToSpawn;
    public float3 spawnBounds;
}