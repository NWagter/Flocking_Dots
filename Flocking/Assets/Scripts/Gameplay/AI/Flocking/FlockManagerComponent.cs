using Unity.Entities;
using Unity.Mathematics;

public struct FlockManagerComponent : IComponentData
{
    public float3 centroid;
}

public struct FlockAgentElement : IBufferElementData
{
    public Entity agent;
}