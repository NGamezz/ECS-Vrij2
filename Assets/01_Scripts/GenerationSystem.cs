using Unity.Burst;
using Unity.Entities;
using Unity.Jobs;

public partial struct GenerationSystem : ISystem
{
    [BurstCompile]
    public void OnUpdate ( ref SystemState state )
    {
        state.Enabled = false;
    }
}

[BurstCompile]
public partial struct GenerateMapJob : IJob
{
    public GenerationConfig generationConfig;

    public EntityCommandBuffer ECB;

    public uint seed;

    [BurstCompile]
    public void Execute ()
    {
    }
}