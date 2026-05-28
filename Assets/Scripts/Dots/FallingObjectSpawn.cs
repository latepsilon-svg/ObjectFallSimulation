using UnityEngine;
using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;

class FallingObjectSpawn : MonoBehaviour
{
    public GameObject prefab;
    public Vector3 spawnPosition;
    public float spawnScale;
}

class FallingObjectSpawnBaker : Baker<FallingObjectSpawn>
{
    public override void Bake(FallingObjectSpawn authoring)
    {
        var entity = GetEntity(TransformUsageFlags.None);

        AddComponent(entity, new FallingObjectSpawner
        {
            Prefab = GetEntity(authoring.prefab, TransformUsageFlags.Dynamic),
            spawnPosition = new float3(authoring.spawnPosition.x, authoring.spawnPosition.y, authoring.spawnPosition.z),
            spawnScale = authoring.spawnScale
        });
    }
}

public struct FallingObjectSpawner : IComponentData
{
    public Entity Prefab;
    public float3 spawnPosition;
    public float spawnScale;
}

public partial struct FallingObjectSpawnSystem : ISystem
{
    public void OnUpdate(ref SystemState state)
    {
        if (Input.GetKey(KeyCode.RightArrow))
        {
            Shader.SetGlobalFloat("_MinLight", math.clamp(Shader.GetGlobalFloat("_MinLight") + 0.01f * SystemAPI.Time.DeltaTime, 0, 1f));
        }
        if (Input.GetKey(KeyCode.LeftArrow))
        {
            Shader.SetGlobalFloat("_MinLight", math.clamp(Shader.GetGlobalFloat("_MinLight") - 0.01f * SystemAPI.Time.DeltaTime, 0, 1f));
        }

        if (!Input.GetKeyDown(KeyCode.Space))
            return;


        var ecb = new EntityCommandBuffer(Unity.Collections.Allocator.Temp);

        foreach (var spawner in
                 SystemAPI.Query<RefRO<FallingObjectSpawner>>())
        {
            Entity obj =
                ecb.Instantiate(spawner.ValueRO.Prefab);

            LocalTransform tr = LocalTransform.Identity;
            tr.Position = spawner.ValueRO.spawnPosition;
            tr.Scale = spawner.ValueRO.spawnScale;

            ecb.SetComponent(
                obj,
                tr
            );
        }

        ecb.Playback(state.EntityManager);
        ecb.Dispose();
    }
}
