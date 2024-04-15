using Unity.Entities;
using UnityEngine;

public class SpawnCubesConfigAuthoring : MonoBehaviour
{
    public GameObject prefab;
    public int amountToSpawn;

    private class Baker : Baker<SpawnCubesConfigAuthoring>
    {
        public override void Bake ( SpawnCubesConfigAuthoring authoring )
        {
            Entity entity = GetEntity(TransformUsageFlags.None);

            AddComponent(entity, new SpawnCubesConfig()
            {
                prefab = GetEntity(authoring.prefab, TransformUsageFlags.Dynamic),
                amountToSpawn = authoring.amountToSpawn,
            });
        }
    }
}

public struct SpawnCubesConfig : IComponentData
{
    public Entity prefab;
    public int amountToSpawn;
}