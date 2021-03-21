using Unity.Entities;
using Unity.Mathematics;

public struct FlockManagerComponent : IComponentData
{

}

public struct FlockAgentElement : IBufferElementData
{
    public Entity agent;
}