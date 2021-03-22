﻿using Unity.Entities;
using Unity.Collections;
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

        var translations = GetComponentDataFromEntity<Translation>(true);
        var flockAgents = GetComponentDataFromEntity<FlockAgentComponent>(false);
        var flockManager = GetComponentDataFromEntity<FlockManagerComponent>(false);

        var query = GetEntityQuery(ComponentType.ReadOnly<FlockManagerComponent>());
        var ents = query.ToEntityArray(Allocator.TempJob);


        Entities.ForEach((
            Entity e,
            ref FlockManagerComponent flockManager,
            ref DynamicBuffer<NeighborAgentElements> neighborBuffer) =>
        {
            neighborBuffer.Clear();

            for (int i = 0;i< ents.Length;i++)
            {
                if(math.distance(translations[ents[i]].Value, translations[e].Value) < 5.0f * 2.0f)
                {
                    var buffer = GetBuffer<FlockAgentElement>(ents[i]);
                    for (int j = 0; j < buffer.Length; j++)
                    {
                        neighborBuffer.Add(new NeighborAgentElements()
                        {
                            agent = buffer[j].agent
                        });
                    }
                }
            }

        }).Run();

        ents.Dispose();

        Entities
            .WithReadOnly(translations)
            .WithReadOnly(flockAgents)
            .ForEach((Entity e, int entityInQueryIndex,
            ref FlockManagerComponent flockManager,
            in DynamicBuffer<NeighborAgentElements> neighborBuffer,
            in DynamicBuffer<FlockAgentElement> agentBuffer) =>
        {
            NativeList<FlockAgentComponent> agents = new NativeList<FlockAgentComponent>(Allocator.Temp);
            float3 centroid = float3.zero;

            for (int i = 0; i < neighborBuffer.Length;i++)
            {
                agents.Add(flockAgents[neighborBuffer[i].agent]);
            }

            //Flock around the flockmanager leader 
            for (int i = 0; i < agentBuffer.Length; i++)
            {
                Entity agent = agentBuffer[i].agent;
                FlockAgentComponent agentComp = flockAgents[agent];

                agentComp.CalculateSpeed(agents);

                var cohesion = agentComp.ComputeCohesion(agents);
                var alignment = agentComp.ComputeAlignment(agents);
                var seperation = agentComp.ComputeSeparation(agents);
                var bounds = agentComp.ComputeBounds(translations[e].Value, 5.0f);

                var vel = (cohesion * 0.1f) + (seperation * 1.0f) + (alignment * 0.8f) + (bounds * 1.0f);
                vel = math.normalize(vel);
                agentComp.velocity = vel;

                ecb.SetComponent(entityInQueryIndex, agent, agentComp);
                centroid += translations[agent].Value;
            }


            flockManager.centroid = (centroid / agentBuffer.Length);

            agents.Dispose();

        }).WithName("Flock_Manager").ScheduleParallel();


        endCommandBuffer.AddJobHandleForProducer(this.Dependency);
    }
}
