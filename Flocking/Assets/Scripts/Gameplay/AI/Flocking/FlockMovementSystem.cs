using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

public class FlockMovementSystem : SystemBase
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


        Entities.ForEach((Entity e, int entityInQueryIndex,
            ref FlockManagerComponent flockManager,
            ref DynamicBuffer<FlockAgentElement> agentBuffer) =>
        {
            //Flock around the flockmanager leader 


        }).ScheduleParallel();


        endCommandBuffer.AddJobHandleForProducer(this.Dependency);
    }
}
