using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public class SpawnCubesConfigAuthoring : MonoBehaviour
{
    public GameObject prefab;
    public int amountToSpawn;

    public float spawnRate;

    public float timeToSpawn;

    public float2 spawnBounds;

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
                spawnRate = authoring.spawnRate,
                timeToSpawn = authoring.timeToSpawn,
            });

            AddComponent(entity, new SpawnTimer()
            {
                Value = authoring.timeToSpawn,
            });
        }
    }
}

public struct SpawnTimer : IComponentData
{
    public float Value;
}

public struct SpawnCubesConfig : IComponentData
{
    public Entity prefab;
    public int amountToSpawn;
    public float2 spawnBounds;
    public float spawnRate;
    public float timeToSpawn;
}