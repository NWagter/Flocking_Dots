using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

public class FormationMoveSystem : SystemBase
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

        float dt = Time.DeltaTime;

        Entities
            .WithAll<FormationComponent>()
            .ForEach((Entity e,
            int entityInQueryIndex,
            ref Translation trans,
            in MoveActionComponent moveAction) =>
        {
            float d = math.distance(trans.Value, moveAction.m_newLocation);

            if(d < 0.25f)
            {
                ecb.RemoveComponent<MoveActionComponent>(entityInQueryIndex, e);
            }
            else
            {
                float3 direction = moveAction.m_newLocation - trans.Value;
                trans.Value += (direction * 5) * dt;
            }

        }).ScheduleParallel();

        m_endCommandBuffer.AddJobHandleForProducer(this.Dependency);
    }
}