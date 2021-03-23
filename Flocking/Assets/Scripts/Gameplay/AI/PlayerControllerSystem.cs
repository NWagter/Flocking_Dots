using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

[AlwaysUpdateSystem]
public class PlayerControllerSystem : SystemBase
{
    private Camera m_cam; 
    private EndSimulationEntityCommandBufferSystem m_endCommandBuffer;

    protected override void OnCreate()
    {
        base.OnCreate();
        m_endCommandBuffer = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
    }
    protected override void OnStartRunning()
    {
        base.OnStartRunning();
        m_cam = Camera.main;
    }

    protected override void OnUpdate()
    {
        RaycastHit hit;
        bool onClick = false;
        float3 point = float3.zero;

        if (Input.GetMouseButtonDown(1))
        {
            onClick = true;
            Ray ray = m_cam.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out hit))
            {
                point = hit.point;
            }
        }

        var ecb = m_endCommandBuffer.CreateCommandBuffer().AsParallelWriter();

        var moveOrders = GetComponentDataFromEntity<MoveActionComponent>(true);

        Entities
            .WithReadOnly(moveOrders)
            .WithAll<FormationComponent>()
            .ForEach((Entity e,
            int entityInQueryIndex) =>
        {
            if(!onClick)
            {
                return;
            }

            if (!moveOrders.HasComponent(e))
            {
                ecb.AddComponent(entityInQueryIndex, e, new MoveActionComponent()
                {
                    m_newLocation = point
                });
            }

        }).ScheduleParallel();

        m_endCommandBuffer.AddJobHandleForProducer(this.Dependency);
    }
}