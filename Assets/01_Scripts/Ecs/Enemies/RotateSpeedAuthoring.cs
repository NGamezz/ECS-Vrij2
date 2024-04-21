using Unity.Entities;
using UnityEngine;

public partial class RotateSpeedAuthoring : MonoBehaviour
{
    public float value;

    public class Baker : Baker<RotateSpeedAuthoring>
    {
        public override void Bake ( RotateSpeedAuthoring authoring )
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);

            AddComponent(entity, new RotateSpeed
            {
                value = authoring.value,
            });
        }
    }
}

public struct RotateSpeed : IComponentData
{
    public float value;
}