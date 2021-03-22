using Unity.Entities;
using Unity.Mathematics;
using Unity.Collections;

public struct FlockAgentComponent : IComponentData
{
    public Entity flockManager;

    public float3 currentLocation;
    public float3 fowardDir;

    public float3 velocity;
    public float speed;

    public void CalculateSpeed(NativeList<FlockAgentComponent> agents)
    {
        if (agents.Length <= 0)
            return;

        speed = 0;

        for (int i = 0; i < agents.Length; i++)
        {
            FlockAgentComponent agent = agents[i];
            speed += agent.speed;
        }

        speed /= agents.Length;

        speed = math.clamp(speed, 1.5f, 10.0f);
    }

    public float3 ComputeAlignment(NativeList<FlockAgentComponent> agents)
    {
        var aligementVector = fowardDir;

        if (agents.Length <= 0)
            return aligementVector;

        int neighboursInFOV = 0;
        for (int i = 0; i < agents.Length; i++)
        {
            FlockAgentComponent agent = agents[i];
            if (agent.currentLocation.Equals(currentLocation))
                continue;

            if (math.distance(agent.currentLocation, currentLocation) < 10 && IsInFOV(agent.currentLocation))
            {
                neighboursInFOV++;
                aligementVector += agent.fowardDir;
            }
        }

        if (neighboursInFOV <= 0)
            return fowardDir;

        aligementVector /= neighboursInFOV;
        aligementVector = math.normalize(aligementVector);
        aligementVector.y = 0;
        return aligementVector;

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

            if (math.distance(agent.currentLocation, currentLocation) < 5.0f && IsInFOV(agent.currentLocation))
            {
                v.x += agent.currentLocation.x;
                v.z += agent.currentLocation.z;
                neighborCount++;
            }
        }

        if (neighborCount <= 0)
            return float3.zero;

        v /= neighborCount;
        v -= currentLocation;
        v = math.normalize(v);
        v.y = 0;
        return v;

    }

    public float3 ComputeSeparation(NativeList<FlockAgentComponent> agents)
    {
        var avoidanceVector = float3.zero;

        if (agents.Length <= 0)
            return avoidanceVector;

        int neighboursInFOV = 0;
        for (int i = 0; i < agents.Length; i++)
        {
            FlockAgentComponent agent = agents[i];
            if (agent.currentLocation.Equals(currentLocation))
                continue;

            if (math.distance(agent.currentLocation, currentLocation) < 2.5f && IsInFOV(agent.currentLocation))
            {
                neighboursInFOV++;
                avoidanceVector += (currentLocation - agent.currentLocation);
            }
        }

        if (neighboursInFOV <= 0 || avoidanceVector.Equals(float3.zero))
            return avoidanceVector;

        avoidanceVector /= neighboursInFOV;
        avoidanceVector = math.normalize(avoidanceVector);
        avoidanceVector.y = 0;
        return avoidanceVector;
    }

    public float3 ComputeBounds(float3 centroid, float boundDistance)
    {
        var offsetToCenter = centroid - currentLocation;
        
        bool isNearCenter = (UnityEngine.Vector3.Magnitude(offsetToCenter) >= boundDistance * 0.9f);
        return isNearCenter ? math.normalize(offsetToCenter) : float3.zero;
    }

    private bool IsInFOV(float3 location)
    {
        return UnityEngine.Vector3.Angle(fowardDir, location - currentLocation) <= 270;
    }
}
