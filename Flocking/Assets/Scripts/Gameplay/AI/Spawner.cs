using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

public class Spawner : MonoBehaviour
{
    [SerializeField]
    private FormationSO formationSO;
    [SerializeField]
    private GameObject formationObject;
    private Entity formationEntity;
    private BlobAssetStore blobAssetStore;

    private EntityManager eManager;
    private Camera cam;

    private void Start()
    {
        cam = Camera.main;
        eManager = World.DefaultGameObjectInjectionWorld.EntityManager;
        blobAssetStore = new BlobAssetStore();

        formationEntity = EntitiesPrefabLibrary.GetInstance().ConvertToEntity(formationObject);
    
        if(SpawnerSystem.instance != null)
        {
            SpawnerSystem.instance.formationEntity = formationEntity;
        }
    }

    private void OnDestroy()
    {
        blobAssetStore.Dispose();
    }

    private void Update()
    {
        if (Input.GetKey(KeyCode.LeftShift) && Input.GetMouseButtonDown(0))
        {
            RaycastHit hit;
            Ray ray = cam.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out hit))
            {
                Entity e = eManager.CreateEntity();
                eManager.AddComponentData(e, new SpawnComponent()
                {
                    spawnLocation = hit.point,
                    formation = formationSO.GetId()
                });
            }
        }

    }

}



public class SpawnerSystem : SystemBase
{
    public static SpawnerSystem instance;

    private BeginSimulationEntityCommandBufferSystem beginCBS;
    public Entity formationEntity;


    protected override void OnCreate()
    {
        instance = this;
        beginCBS = World.GetOrCreateSystem<BeginSimulationEntityCommandBufferSystem>();
    }

    protected override void OnUpdate()
    {
        var bcb = beginCBS.CreateCommandBuffer();

        Entities.ForEach((Entity e, ref SpawnComponent sComp) =>
        {
            Entity leader = bcb.Instantiate(formationEntity);
            var formation = FormationLibrary.GetInstance().GetContent<FormationSO>(sComp.formation);         

            bcb.SetComponent<Translation>(leader, new Translation()
            {
                Value = sComp.spawnLocation
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
                    Entity ent = SpawnUnit(sComp.spawnLocation + new float3(x * 2, 0, y * 2), formation.unit, bcb);

                    bcb.AddComponent(ent, new FlockAgentComponent()
                    {
                        flockManager = leader,
                        desiredLocation = new float3(x * 2, 0, y * 2)
                    });

                    buffer.Add(new FlockAgentElement()
                    {
                        agent = ent
                    });
                }
            }


            bcb.DestroyEntity(e);
        }).WithoutBurst().Run();

        beginCBS.AddJobHandleForProducer(this.Dependency);
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
