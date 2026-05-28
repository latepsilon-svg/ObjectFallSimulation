using System.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine;

[MaterialProperty("_Speed")]
public struct ParticleVisual : IComponentData
{
    public float Speed;
}

public struct ParticleSubData : IComponentData
{
    public float MaxMomentumColorization;
    public float pressureStrength;
    public float noiseStrength;
    public float drag;
    public float domain;
    public int modosexo;
}

public partial struct ParticleVisualSystem : ISystem
{
    public void OnUpdate(ref SystemState state)
    {
        foreach (var (velocity, visual, sub, transform)
                 in SystemAPI.Query<
                     RefRW<PhysicsVelocity>,
                     RefRW<ParticleVisual>,
                     RefRO<ParticleSubData>,
                     RefRO<LocalTransform>>())
        {
            visual.ValueRW.Speed =
                math.clamp(math.length(velocity.ValueRO.Linear) / sub.ValueRO.MaxMomentumColorization, 0, 1);


            float3 pos = transform.ValueRO.Position;

            float distance = math.length(pos);

            float3 dirToCenter =
                -math.normalize(pos);

            float nd = distance >= sub.ValueRO.domain ? distance / sub.ValueRO.domain : 0;

            float force =
                nd * nd * sub.ValueRO.pressureStrength;

            velocity.ValueRW.Linear +=
                dirToCenter * force * SystemAPI.Time.DeltaTime;

            float am = math.length(velocity.ValueRO.Linear);

            float3 drdir = am * velocity.ValueRO.Linear;

            float dr = distance * sub.ValueRO.drag / (sub.ValueRO.domain / 2);

            velocity.ValueRW.Linear -=
                drdir * (sub.ValueRO.modosexo == 1 ? 0 : 1)
                * dr
                * SystemAPI.Time.DeltaTime;

            float3 noise = new float3(
                UnityEngine.Random.Range(-1f, 1f),
                UnityEngine.Random.Range(-1f, 1f),
                UnityEngine.Random.Range(-1f, 1f)
            );

            velocity.ValueRW.Linear +=
                noise
                * sub.ValueRO.noiseStrength
                * SystemAPI.Time.DeltaTime;
        }
    }
}