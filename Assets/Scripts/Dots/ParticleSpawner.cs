using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;
using UnityEngine;

public class ParticleSpawner : MonoBehaviour
{
    [Range(1, 10)]
    public int quality;
    [Space(10)]
    public GameObject prefab;
    public int count;
    public float cubeSize = 10;
    public float scale = .1f;
    public float initialMomentum = 0.0f;
    public float maxMomentumColorization = 4;
    public float pressureStrenght = 1.0f;
    public bool modosexo = false;
    public float drag = 0.1f;
    public float noiseStrenght = 0.2f;
    public float domain = 10;
}

public class ParticleSpawnerBaker : Baker<ParticleSpawner>
{
    public override void Bake(ParticleSpawner authoring)
    {
        var entity = GetEntity(TransformUsageFlags.None);

        float nv = authoring.quality;
        nv /= 10;

        AddComponent(entity, new ParticleSpawnerComponent
        {
            Prefab = GetEntity(authoring.prefab, TransformUsageFlags.Dynamic),
            Count = (int)(authoring.count * nv),
            Scale = authoring.scale * (0.5f + nv / 2),
            CubeSize = authoring.cubeSize,
            InitialMomentum = authoring.initialMomentum,
            MaxMomentumColorization = authoring.maxMomentumColorization,
            PressureStrenght = authoring.pressureStrenght,
            sex = authoring.modosexo ? 1 : 0,
            noiseStrength = authoring.noiseStrenght,
            drag = authoring.drag,
            domain = authoring.domain,
        });
    }
}

public struct ParticleSpawnerComponent : IComponentData
{
    public Entity Prefab;
    public int Count;
    public float Scale;
    public float CubeSize;
    public float InitialMomentum;
    public float MaxMomentumColorization;
    public float PressureStrenght;
    public int sex;
    public float noiseStrength;
    public float drag;
    public float domain;
}


public partial struct ParticleSpawnSystem : ISystem
{
    public void OnUpdate(ref SystemState state)
    {
        var ecb = new EntityCommandBuffer(Allocator.Temp);

        foreach (var (spawner, entity) in
                 SystemAPI.Query<
                     RefRO<ParticleSpawnerComponent>>()
                 .WithEntityAccess())
        {
            for (int i = 0; i < spawner.ValueRO.Count; i++)
            {
                Entity particle =
                    ecb.Instantiate(spawner.ValueRO.Prefab);

                float3 randomPos = new float3(
                    UnityEngine.Random.Range(-spawner.ValueRO.CubeSize, spawner.ValueRO.CubeSize),
                    UnityEngine.Random.Range(-spawner.ValueRO.CubeSize, spawner.ValueRO.CubeSize),
                    UnityEngine.Random.Range(-spawner.ValueRO.CubeSize, spawner.ValueRO.CubeSize)
                );

                LocalTransform transform = LocalTransform.Identity;

                transform.Position = randomPos;
                transform.Scale = spawner.ValueRO.Scale;

                ecb.SetComponent(particle, transform);

                float3 randomVelocity = new float3(0, 0, 0);

                if (spawner.ValueRO.sex == 1)
                {
                    randomVelocity = new float3(
                        (UnityEngine.Random.value > 0.5f ? 1 : -1) * spawner.ValueRO.InitialMomentum,
                        (UnityEngine.Random.value > 0.5f ? 1 : -1) * spawner.ValueRO.InitialMomentum,
                        (UnityEngine.Random.value > 0.5f ? 1 : -1) * spawner.ValueRO.InitialMomentum
                    );
                }
                else
                {
                    randomVelocity = new float3(
                        (2 * (UnityEngine.Random.value - 0.5f)) * spawner.ValueRO.InitialMomentum,
                        (2 * (UnityEngine.Random.value - 0.5f)) * spawner.ValueRO.InitialMomentum,
                        (2 * (UnityEngine.Random.value - 0.5f)) * spawner.ValueRO.InitialMomentum
                    );
                }


                ecb.SetComponent(
                    particle,
                    new PhysicsVelocity
                    {
                        Linear = randomVelocity,
                        Angular = float3.zero
                    }
                );

                ecb.AddComponent(
                    particle,
                    new ParticleVisual
                    {
                        Speed = 0,
                    });

                ecb.AddComponent(
                    particle,
                    new ParticleSubData
                    {
                        MaxMomentumColorization = spawner.ValueRO.MaxMomentumColorization,
                        pressureStrength = spawner.ValueRO.PressureStrenght,
                        noiseStrength = spawner.ValueRO.noiseStrength,
                        drag = spawner.ValueRO.drag,
                        domain = spawner.ValueRO.domain,
                        modosexo = spawner.ValueRO.sex,
                    });

            }

            ecb.DestroyEntity(entity);
        }

        ecb.Playback(state.EntityManager);
        ecb.Dispose();
    }
}