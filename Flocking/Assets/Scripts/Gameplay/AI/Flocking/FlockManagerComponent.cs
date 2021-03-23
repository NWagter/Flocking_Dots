using Unity.Entities;
using Unity.Mathematics;

public struct FlockManagerComponent : IComponentData
{
    public float3 m_centroid;
}

public struct FlockAgentElement : IBufferElementData
{
    public Entity m_agent;
}
public struct NeighborAgentElements : IBufferElementData
{
    public Entity m_agent;
}