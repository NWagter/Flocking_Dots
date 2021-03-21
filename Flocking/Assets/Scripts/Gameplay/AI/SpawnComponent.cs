using System;
using Unity.Entities;
using Unity.Mathematics;

struct SpawnComponent : IComponentData
{
    public float3 spawnLocation;
    public Guid formation;
}