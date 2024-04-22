using Unity.Entities;
using UnityEngine;

public class PlayerAuthoring : MonoBehaviour
{
    private class Baker : Baker<PlayerAuthoring>
    {
        public override void Bake ( PlayerAuthoring authoring )
        {
            Entity entity = GetEntity(TransformUsageFlags.None);

            AddComponent(entity, new Player());
        }
    }
}

public struct Player : IComponentData
{
}