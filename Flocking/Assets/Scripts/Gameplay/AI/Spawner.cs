using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
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
        if (Input.GetMouseButtonDown(0))
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
            Entity newE = bcb.Instantiate(formationEntity);
            var formation = FormationLibrary.GetInstance().GetContent<FormationSO>(sComp.formation);

            

            bcb.SetComponent<Translation>(newE, new Translation()
            {
                Value = sComp.spawnLocation
            });

            bcb.DestroyEntity(e);
        }).WithoutBurst().Run();

        beginCBS.AddJobHandleForProducer(this.Dependency);
    }
}
