using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

public class FlockAgentSystem : SystemBase
{
    private EndSimulationEntityCommandBufferSystem endCommandBuffer;

    protected override void OnCreate()
    {
        base.OnCreate();
        endCommandBuffer = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
    }

    protected override void OnUpdate()
    {
        var ecb = endCommandBuffer.CreateCommandBuffer().AsParallelWriter();

        float dT = Time.DeltaTime;

        Entities
            .ForEach((Entity e, int entityInQueryIndex,
            ref FlockAgentComponent flockAgent,
            ref Translation trans,
            ref Rotation rot,
            ref LocalToWorld lTW) =>
        {
            if (flockAgent.velocity.Equals(float3.zero))
                flockAgent.velocity = lTW.Forward;

            flockAgent.currentLocation = trans.Value;

            trans.Value += flockAgent.velocity * dT;

        }).WithName("Flock_Agent").ScheduleParallel();


        endCommandBuffer.AddJobHandleForProducer(this.Dependency);
    }
}
