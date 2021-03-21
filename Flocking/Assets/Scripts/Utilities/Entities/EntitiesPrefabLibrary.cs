using System;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;

public class EntitiesPrefabLibrary : MonoBehaviour
{
    #region Singleton
    public static EntitiesPrefabLibrary GetInstance()
    {
        if (instance == null)
        {
            instance = FindObjectOfType<EntitiesPrefabLibrary>();
        }
        return instance;
    }

    private static EntitiesPrefabLibrary instance;
    #endregion

    private Dictionary<Guid, Entity> m_entityPrefabs = new Dictionary<Guid, Entity>();
    private BlobAssetStore m_blobAssetStore;
    private EntityManager m_entityManager;

    private void Awake()
    {
        m_entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
    }

    public Entity ConvertToEntity(GameObject a_prefab)
    {
        GameObjectConversionSettings settings = GameObjectConversionSettings.FromWorld(World.DefaultGameObjectInjectionWorld, m_blobAssetStore);
        Entity entityPrefab = GameObjectConversionUtility.ConvertGameObjectHierarchy(a_prefab, settings);

        return entityPrefab;
    }

    private void OnEnable()
    {
        m_blobAssetStore = new BlobAssetStore();
    }

    private void OnDisable()
    {
        m_blobAssetStore.Dispose();
    }

    public void ResetEntityPrefabs()
    {
        EntityManager entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;

        foreach (var keyValuePair in m_entityPrefabs)
        {
            m_entityManager.DestroyEntity(keyValuePair.Value);
        }

        m_entityPrefabs.Clear();
    }

    private void OnDestroy()
    {
        Destroy(instance);
        instance = null;
    }
}
