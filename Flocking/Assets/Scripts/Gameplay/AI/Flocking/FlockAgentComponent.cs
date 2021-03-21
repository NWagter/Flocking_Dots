using Unity.Entities;
using Unity.Mathematics;
using Unity.Collections;

public struct FlockAgentComponent : IComponentData
{
    public Entity flockManager;

    public float3 currentLocation;

    public float3 velocity;
    public float speed;

    public float3 ComputeAlignment(NativeList<FlockAgentComponent> agents)
    {
        int neighborCount = 0;
        float3 v = velocity;

        for (int i = 0; i < agents.Length; i++)
        {
            FlockAgentComponent agent = agents[i];

            if (agent.currentLocation.Equals(currentLocation))
                continue;

            if (math.distance(agent.currentLocation, currentLocation) < 5)
            {
                v.x += agent.velocity.x;
                v.z += agent.velocity.z;
                neighborCount++;
            }
        }

        if (neighborCount == 0)
            velocity = v;

        v.x /= neighborCount;
        v.y = 0;
        v.z /= neighborCount;
        return math.normalize(v);

    }
    public float3 ComputeCohesion(NativeList<FlockAgentComponent> agents)
    {
        int neighborCount = 0;
        float3 v = float3.zero;

        for (int i = 0; i < agents.Length; i++)
        {
            FlockAgentComponent agent = agents[i];

            if (agent.currentLocation.Equals(currentLocation))
                continue;

            if (math.distance(agent.currentLocation, currentLocation) < 5)
            {
                v.x += agent.currentLocation.x;
                v.z += agent.currentLocation.z;
                neighborCount++;
            }
        }

        v.x /= neighborCount;
        v.y = 0;
        v.z /= neighborCount;
        return math.normalize(new float3(v.x - currentLocation.x, 0, v.z - currentLocation.z));

    }
    public float3 ComputeSeparation(NativeList<FlockAgentComponent> agents)
    {
        int neighborCount = 0;
        float3 v = float3.zero;

        for (int i = 0; i < agents.Length; i++)
        {
            FlockAgentComponent agent = agents[i];

            if (agent.currentLocation.Equals(currentLocation))
                continue;

            if (math.distance(agent.currentLocation, currentLocation) < 5)
            {
                v.x += agent.currentLocation.x - currentLocation.x;
                v.z += agent.currentLocation.z - currentLocation.z;
                neighborCount++;
            }
        }

        v.x /= neighborCount;
        v.y = 0;
        v.z /= neighborCount;

        v.x *= -1;
        v.z *= -1;

        return v;
    }

}
