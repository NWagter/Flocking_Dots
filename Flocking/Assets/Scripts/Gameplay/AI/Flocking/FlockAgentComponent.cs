using Unity.Entities;
using Unity.Mathematics;
using Unity.Collections;

public struct FlockAgentComponent : IComponentData
{
    public Entity m_flockManager;
    public float3 m_desiredLocation;

    public float3 m_currentLocation;
    public float3 m_fowardDir;

    public float3 m_velocity;
    public float m_speed;

    public void CalculateSpeed(NativeList<FlockAgentComponent> agents)
    {
        if (agents.Length <= 0)
            return;

        m_speed = 0;

        for (int i = 0; i < agents.Length; i++)
        {
            FlockAgentComponent agent = agents[i];
            m_speed += agent.m_speed;
        }

        m_speed /= agents.Length;

        m_speed = math.clamp(m_speed, 3.0f, 7.0f);
    }

    public float3 ComputeAlignment(NativeList<FlockAgentComponent> agents)
    {
        var aligementVector = m_fowardDir;

        if (agents.Length <= 0)
            return aligementVector;

        int neighboursInFOV = 0;
        for (int i = 0; i < agents.Length; i++)
        {
            FlockAgentComponent agent = agents[i];
            if (agent.m_currentLocation.Equals(m_currentLocation))
                continue;

            if (math.distance(agent.m_currentLocation, m_currentLocation) < 3.0f && IsInFOV(agent.m_currentLocation))
            {
                neighboursInFOV++;
                aligementVector += agent.m_fowardDir;
            }
        }

        if (neighboursInFOV <= 0)
            return m_fowardDir;

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
            if (agent.m_currentLocation.Equals(m_currentLocation))
                continue;

            if (math.distance(agent.m_currentLocation, m_currentLocation) < 2.0f && IsInFOV(agent.m_currentLocation))
            {
                v.x += agent.m_currentLocation.x;
                v.z += agent.m_currentLocation.z;
                neighborCount++;
            }
        }

        if (neighborCount <= 0)
            return float3.zero;

        v /= neighborCount;
        v -= m_currentLocation;
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
            if (agent.m_currentLocation.Equals(m_currentLocation))
                continue;

            if (math.distance(agent.m_currentLocation, m_currentLocation) < 3.5f && IsInFOV(agent.m_currentLocation))
            {
                neighboursInFOV++;
                avoidanceVector += (m_currentLocation - agent.m_currentLocation);
            }
        }

        if (neighboursInFOV <= 0 || avoidanceVector.Equals(float3.zero))
            return avoidanceVector;

        avoidanceVector /= neighboursInFOV;
        avoidanceVector = math.normalize(avoidanceVector);
        avoidanceVector.y = 0;
        return avoidanceVector;
    }

    public float3 CompoteDesired(float3 formationLocation)
    {
        float3 desired = (formationLocation + m_desiredLocation);
        float d = math.distance(desired, m_currentLocation);
        if (d < 0.25f || m_velocity.Equals(float3.zero))
        {
            return float3.zero;
        }

        float3 v = (m_desiredLocation + formationLocation) - m_currentLocation;

        v = v - m_velocity;
        v = math.normalize(v);
        return v;
    }

    public float3 ComputeBounds(float3 centroid, float boundDistance)
    {
        var offsetToCenter = centroid - m_currentLocation;
        
        bool isNearCenter = (UnityEngine.Vector3.Magnitude(offsetToCenter) >= boundDistance * 0.9f);
        return isNearCenter ? math.normalize(offsetToCenter) : float3.zero;
    }

    private bool IsInFOV(float3 location)
    {
        return UnityEngine.Vector3.Angle(m_fowardDir, location - m_currentLocation) <= 320;
    }
}
