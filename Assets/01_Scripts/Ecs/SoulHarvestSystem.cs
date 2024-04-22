using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

[BurstCompile]
public partial class SoulHarvestSystem : SystemBase
{
    protected override void OnCreate ()
    {
        RequireForUpdate<SoulHarvestPillar>();
    }

    [BurstCompile]
    protected override void OnUpdate ()
    {
        Enabled = false;
        return;

        foreach ( var (pillar, transform, pillarEntity) in SystemAPI.Query<RefRW<SoulHarvestPillar>, RefRO<LocalTransform>>().WithDisabled<PillarCompleted>().WithEntityAccess() )
        {
            foreach ( var (localTransformm, entity) in SystemAPI.Query<RefRO<LocalTransform>>().WithAll<SoulHarvest>().WithEntityAccess() )
            {
                float3 entityPos = localTransformm.ValueRO.Position;

                var direction = entityPos - transform.ValueRO.Position;

                var lenght = math.length(direction);

                if ( lenght < pillar.ValueRO.radius )
                {
                    UnityEngine.Debug.Log(pillar.ValueRO.souls);
                    pillar.ValueRW.souls += 1;
                }

                if ( pillar.ValueRO.souls >= pillar.ValueRO.desiredSouls )
                {
                    EventManagerGeneric<Entity>.InvokeEvent(EventType.UponDesiredSoulsAmount, pillarEntity);
                    SystemAPI.SetComponentEnabled<PillarCompleted>(pillarEntity, true);
                }
                SystemAPI.SetComponentEnabled<SoulHarvest>(entity, false);
            }
        }
    }
}