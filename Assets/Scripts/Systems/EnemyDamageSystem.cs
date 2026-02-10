using ArcadeShooter.Core;
using ArcadeShooter.Data;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics.Systems;
using Unity.Transforms;

namespace ArcadeShooter.Systems
{
    [BurstCompile]
    [UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
    [UpdateAfter(typeof(PhysicsSystemGroup))]
    public partial struct EnemyDamageSystem : ISystem
    {
        [BurstCompile]
        [WithAll(typeof(EnemyTag))]
        public partial struct EnemyDamageJob : IJobEntity
        {
            public float3 PlayerPosition;
            public float DamageRangeSq;
            public float DeltaTime;
            public NativeQueue<float>.ParallelWriter DamageQueue;

            public void Execute(RefRO<LocalTransform> transform, RefRO<DamageData> damageData)
            {
                float3 enemyPosition = transform.ValueRO.Position;
                float distanceSq = math.distancesq(PlayerPosition, enemyPosition);

                if (distanceSq <= DamageRangeSq)
                {
                    DamageQueue.Enqueue(damageData.ValueRO.Value * DeltaTime);
                }
            }
        }

        [BurstCompile]
        [WithAll(typeof(PlayerTag))]
        public partial struct ApplyDamageJob : IJobEntity
        {
            public NativeQueue<float> DamageQueue;

            public void Execute(RefRW<HealthData> health)
            {
                float totalDamage = 0f;
                while (DamageQueue.TryDequeue(out float dmg))
                {
                    totalDamage += dmg;
                }
                if (totalDamage > 0f)
                {
                    health.ValueRW.Current -= totalDamage;
                    health.ValueRW.Current = math.max(0f, health.ValueRW.Current);
                }
            }
        }

        private NativeQueue<float> damageQueue;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<PlayerTag>();
            state.RequireForUpdate<GameConfigData>();
            damageQueue = new NativeQueue<float>(Allocator.Persistent);
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state)
        {
            if (damageQueue.IsCreated)
            {
                damageQueue.Dispose();
            }
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            Entity playerEntity = SystemAPI.GetSingletonEntity<PlayerTag>();
            LocalTransform playerTransform = SystemAPI.GetComponent<LocalTransform>(playerEntity);
            float3 playerPosition = playerTransform.Position;

            GameConfigData gameConfig = SystemAPI.GetSingleton<GameConfigData>();
            float damageRangeSq = gameConfig.EnemyDamageRange * gameConfig.EnemyDamageRange;
            float deltaTime = SystemAPI.Time.DeltaTime;

            EnemyDamageJob damageJob = new EnemyDamageJob
            {
                PlayerPosition = playerPosition,
                DamageRangeSq = damageRangeSq,
                DeltaTime = deltaTime,
                DamageQueue = damageQueue.AsParallelWriter()
            };
            state.Dependency = damageJob.ScheduleParallel(state.Dependency);

            ApplyDamageJob applyJob = new ApplyDamageJob
            {
                DamageQueue = damageQueue
            };
            state.Dependency = applyJob.Schedule(state.Dependency);
        }
    }
}