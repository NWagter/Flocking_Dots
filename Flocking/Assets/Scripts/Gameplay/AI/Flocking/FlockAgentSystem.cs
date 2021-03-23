using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

public class FlockAgentSystem : SystemBase
{
    private EndSimulationEntityCommandBufferSystem m_endCommandBuffer;

    protected override void OnCreate()
    {
        base.OnCreate();
        m_endCommandBuffer = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
    }

    protected override void OnUpdate()
    {
        var ecb = m_endCommandBuffer.CreateCommandBuffer().AsParallelWriter();

        float dT = Time.DeltaTime;

        Entities
            .ForEach((Entity e, int entityInQueryIndex,
            ref FlockAgentComponent flockAgent,
            ref Translation trans,
            ref Rotation rot,
            ref LocalToWorld lTW) =>
        {
            var forward = flockAgent.m_velocity;

            if(!forward.Equals(float3.zero))
            {
                rot.Value = quaternion.LookRotation(forward, lTW.Up);
                flockAgent.m_fowardDir = lTW.Forward;
                flockAgent.m_currentLocation = trans.Value;

                trans.Value += (forward * flockAgent.m_speed )* dT;
            }

        }).WithName("Flock_Agent").ScheduleParallel();


        m_endCommandBuffer.AddJobHandleForProducer(this.Dependency);
    }
}
