using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

public class FormationMoveSystem : SystemBase
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

        float dt = Time.DeltaTime;

        Entities
            .WithAll<FormationComponent>()
            .ForEach((Entity e,
            int entityInQueryIndex,
            ref Translation trans,
            in MoveActionComponent moveAction) =>
        {
            float d = math.distance(trans.Value, moveAction.newLocation);

            if(d < 0.25f)
            {
                ecb.RemoveComponent<MoveActionComponent>(entityInQueryIndex, e);
            }
            else
            {
                float3 direction = moveAction.newLocation - trans.Value;
                trans.Value += (direction * 5) * dt;
            }

        }).ScheduleParallel();

        endCommandBuffer.AddJobHandleForProducer(this.Dependency);
    }
}