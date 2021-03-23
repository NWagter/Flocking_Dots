using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

public class Spawner : MonoBehaviour
{
    [SerializeField]
    private FormationSO m_formationSO;
    [SerializeField]
    private GameObject m_formationObject;
    private Entity m_formationEntity;
    private BlobAssetStore m_blobAssetStore;

    private EntityManager m_eManager;
    private Camera m_cam;

    private void Start()
    {
        m_cam = Camera.main;
        m_eManager = World.DefaultGameObjectInjectionWorld.EntityManager;
        m_blobAssetStore = new BlobAssetStore();

        m_formationEntity = EntitiesPrefabLibrary.GetInstance().ConvertToEntity(m_formationObject);
    
        if(SpawnerSystem.ms_instance != null)
        {
            SpawnerSystem.ms_instance.m_formationEntity = m_formationEntity;
        }
    }

    private void OnDestroy()
    {
        m_blobAssetStore.Dispose();
    }

    private void Update()
    {
        if (Input.GetKey(KeyCode.LeftShift) && Input.GetMouseButtonDown(0))
        {
            RaycastHit hit;
            Ray ray = m_cam.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out hit))
            {
                Entity e = m_eManager.CreateEntity();
                m_eManager.AddComponentData(e, new SpawnComponent()
                {
                    m_spawnLocation = hit.point,
                    m_formation = m_formationSO.GetId()
                });
            }
        }

    }

}



public class SpawnerSystem : SystemBase
{
    public static SpawnerSystem ms_instance;

    private BeginSimulationEntityCommandBufferSystem m_beginCBS;
    public Entity m_formationEntity;


    protected override void OnCreate()
    {
        ms_instance = this;
        m_beginCBS = World.GetOrCreateSystem<BeginSimulationEntityCommandBufferSystem>();
    }

    protected override void OnUpdate()
    {
        var bcb = m_beginCBS.CreateCommandBuffer();

        Entities.ForEach((Entity e, ref SpawnComponent sComp) =>
        {
            Entity leader = bcb.Instantiate(m_formationEntity);
            var formation = FormationLibrary.GetInstance().GetContent<FormationSO>(sComp.m_formation);         

            bcb.SetComponent<Translation>(leader, new Translation()
            {
                Value = sComp.m_spawnLocation
            });

            bcb.AddComponent(leader, new FlockManagerComponent());
            bcb.AddComponent(leader, new FormationComponent());
            DynamicBuffer<FlockAgentElement> buffer = bcb.AddBuffer<FlockAgentElement>(leader);
            bcb.AddBuffer<NeighborAgentElements>(leader);

            float xWidth = (float)formation.formationWidth / 2;
            float yHeight = (float)formation.formationHeight / 2;

            for (float x = -xWidth; x < xWidth; x++)
            {
                for (float y = -yHeight; y < yHeight; y++)
                {
                    Entity ent = SpawnUnit(sComp.m_spawnLocation + new float3(x * 2, 0, y * 2), formation.unit, bcb);

                    bcb.AddComponent(ent, new FlockAgentComponent()
                    {
                        m_flockManager = leader,
                        m_desiredLocation = new float3(x * 2, 0, y * 2)
                    });

                    buffer.Add(new FlockAgentElement()
                    {
                        m_agent = ent
                    });
                }
            }


            bcb.DestroyEntity(e);
        }).WithoutBurst().Run();

        m_beginCBS.AddJobHandleForProducer(this.Dependency);
    }

    public static Entity SpawnUnit(float3 location, UnitSO unit, EntityCommandBuffer bcb)
    {
        Entity ent = bcb.Instantiate(unit.entityPrefab);

        bcb.SetComponent(ent, new Translation()
        {
            Value = location
        }); 

        return ent;
    }
}
