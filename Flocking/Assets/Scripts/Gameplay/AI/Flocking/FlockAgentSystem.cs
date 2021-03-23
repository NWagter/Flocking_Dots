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
            var forward = flockAgent.velocity;

            if(!forward.Equals(float3.zero))
            {
                rot.Value = quaternion.LookRotation(forward, lTW.Up);
                flockAgent.fowardDir = lTW.Forward;
                flockAgent.currentLocation = trans.Value;

                trans.Value += (forward * flockAgent.speed )* dT;
            }

        }).WithName("Flock_Agent").ScheduleParallel();


        endCommandBuffer.AddJobHandleForProducer(this.Dependency);
    }
}
