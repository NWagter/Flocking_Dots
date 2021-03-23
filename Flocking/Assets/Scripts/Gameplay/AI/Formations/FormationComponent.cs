using Unity.Entities;
using Unity.Mathematics;

public struct FormationComponent : IComponentData
{

}

public struct MoveActionComponent : IComponentData
{
    public float3 newLocation;
}