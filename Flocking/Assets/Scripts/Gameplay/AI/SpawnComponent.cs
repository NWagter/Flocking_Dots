using System;
using Unity.Entities;
using Unity.Mathematics;

struct SpawnComponent : IComponentData
{
    public float3 m_spawnLocation;
    public Guid m_formation;
}