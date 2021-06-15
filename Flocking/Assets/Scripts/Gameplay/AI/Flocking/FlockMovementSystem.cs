using Unity.Entities;
using Unity.Collections;
using Unity.Mathematics;
using Unity.Transforms;

public class FlockMovementSystem : SystemBase
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
                            m_agent = buffer[j].m_agent
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
                agents.Add(flockAgents[neighborBuffer[i].m_agent]);
            }

            //Flock around the flockmanager leader 
            for (int i = 0; i < agentBuffer.Length; i++)
            {
                Entity agent = agentBuffer[i].m_agent;
                FlockAgentComponent agentComp = flockAgents[agent];

                agentComp.CalculateSpeed(agents);

                var cohesion = agentComp.ComputeCohesion(agents);
                var alignment = agentComp.ComputeAlignment(agents);
                var seperation = agentComp.ComputeSeparation(agents);
                var bounds = agentComp.ComputeBounds(translations[e].Value, 5.0f);
                var desired = agentComp.CompoteDesired(translations[e].Value);

                var vel = (cohesion * flockManager.cohesionBias) + (seperation * flockManager.seperationBias) + (alignment * flockManager.alignmentBias) + (bounds * flockManager.boundsBias) + (desired * flockManager.desiredBias);
                vel = math.normalize(vel);

                agentComp.m_velocity = vel;

                ecb.SetComponent(entityInQueryIndex, agent, agentComp);
                centroid += translations[agent].Value;
            }


            flockManager.m_centroid = (centroid / agentBuffer.Length);

            agents.Dispose();

        }).WithName("Flock_Manager").ScheduleParallel();


        m_endCommandBuffer.AddJobHandleForProducer(this.Dependency);
    }
}
