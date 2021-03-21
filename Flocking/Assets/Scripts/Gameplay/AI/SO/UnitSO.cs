using System;
using Unity.Entities;
using UnityEngine;


[CreateAssetMenu(fileName = "Unit", menuName = "Units/UnitSO", order = 1)]
public class UnitSO : ContentSO
{
    [SerializeField]
    private GameObject unitObject;

    private Entity _entityPrefab;

    public Entity entityPrefab => _entityPrefab;
    public override void Init()
    {
        base.Init();
        _entityPrefab = EntitiesPrefabLibrary.GetInstance().ConvertToEntity(unitObject);
    }
}