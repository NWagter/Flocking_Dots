using Unity.Entities;
using Unity.Mathematics;

public struct FlockManagerComponent : IComponentData
{
    public float3 m_centroid;

    public float cohesionBias;
    public float seperationBias;
    public float alignmentBias;
    public float boundsBias;
    public float desiredBias;
}

public struct FlockAgentElement : IBufferElementData
{
    public Entity m_agent;
}
public struct NeighborAgentElements : IBufferElementData
{
    public Entity m_agent;
}