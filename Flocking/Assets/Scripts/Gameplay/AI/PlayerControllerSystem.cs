using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

[AlwaysUpdateSystem]
public class PlayerControllerSystem : SystemBase
{
    private Camera cam; 
    private EndSimulationEntityCommandBufferSystem endCommandBuffer;

    protected override void OnCreate()
    {
        base.OnCreate();
        endCommandBuffer = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
    }
    protected override void OnStartRunning()
    {
        base.OnStartRunning();
        cam = Camera.main;
    }

    protected override void OnUpdate()
    {
        RaycastHit hit;
        bool onClick = false;
        float3 point = float3.zero;

        if (Input.GetMouseButtonDown(1))
        {
            onClick = true;
            Ray ray = cam.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out hit))
            {
                point = hit.point;
            }
        }

        var ecb = endCommandBuffer.CreateCommandBuffer().AsParallelWriter();

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
                    newLocation = point
                });
            }

        }).ScheduleParallel();

        endCommandBuffer.AddJobHandleForProducer(this.Dependency);
    }
}