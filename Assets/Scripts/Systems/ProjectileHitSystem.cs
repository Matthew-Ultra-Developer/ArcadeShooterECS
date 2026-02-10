using ArcadeShooter.Core;
using ArcadeShooter.Data;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Physics.Systems;
using Unity.Transforms;

namespace ArcadeShooter.Systems
{
    [BurstCompile]
    [UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
    [UpdateAfter(typeof(PhysicsSystemGroup))]
    public partial struct ProjectileHitSystem : ISystem
    {
        public struct HitInfo
        {
            public Entity ProjectileEntity;
            public float Damage;
        }

        [BurstCompile]
        public struct ProjectileHitTriggerJob : ITriggerEventsJob
        {
            [ReadOnly] public ComponentLookup<ProjectileTag> ProjectileLookup;
            [ReadOnly] public ComponentLookup<EnemyTag> EnemyLookup;
            [ReadOnly] public ComponentLookup<DamageData> DamageLookup;
            public NativeParallelMultiHashMap<Entity, HitInfo>.ParallelWriter HitMap;

            public void Execute(TriggerEvent triggerEvent)
            {
                Entity entityA = triggerEvent.EntityA;
                Entity entityB = triggerEvent.EntityB;
                Entity projectile = Entity.Null;
                Entity enemy = Entity.Null;

                if ((ProjectileLookup.HasComponent(entityA)) && (EnemyLookup.HasComponent(entityB)))
                {
                    projectile = entityA;
                    enemy = entityB;
                }
                else if ((ProjectileLookup.HasComponent(entityB)) && (EnemyLookup.HasComponent(entityA)))
                {
                    projectile = entityB;
                    enemy = entityA;
                }

                if ((projectile != Entity.Null) && (enemy != Entity.Null))
                {
                    float damage = 0f;
                    if (DamageLookup.HasComponent(projectile))
                    {
                        damage = DamageLookup[projectile].Value;
                    }

                    HitMap.Add(enemy, new HitInfo
                    {
                        ProjectileEntity = projectile,
                        Damage = damage
                    });
                }
            }
        }

        [BurstCompile]
        public partial struct ApplyDamageJob : IJobEntity
        {
            public EntityCommandBuffer.ParallelWriter ECB;
            [ReadOnly] public NativeParallelMultiHashMap<Entity, HitInfo> HitMap;
            [ReadOnly] public ComponentLookup<LocalTransform> LocalTransformLookup;
            public GameConfigData GameConfig;
            public CoinSpawnerConfig CoinConfig;
            public RandomComponent Random;

            public void Execute(Entity entity, [EntityIndexInQuery] int sortKey, ref HealthData health)
            {
                if (!HitMap.ContainsKey(entity))
                {
                    return;
                }

                float totalDamage = 0f;
                NativeParallelMultiHashMap<Entity, HitInfo>.Enumerator enumerator = HitMap.GetValuesForKey(entity);
                FixedList128Bytes<Entity> processedProjectiles = new FixedList128Bytes<Entity>();

                while (enumerator.MoveNext())
                {
                    HitInfo hit = enumerator.Current;
                    if (processedProjectiles.Contains(hit.ProjectileEntity))
                    {
                        continue;
                    }
                    if (processedProjectiles.Length < processedProjectiles.Capacity)
                    {
                        processedProjectiles.Add(hit.ProjectileEntity);
                    }

                    totalDamage += hit.Damage;
                    ECB.DestroyEntity(sortKey, hit.ProjectileEntity);
                }

                health.Current -= totalDamage;

                if (health.Current <= 0)
                {
                    Unity.Mathematics.Random random = new Unity.Mathematics.Random((uint)(Random.Value.NextUInt() + sortKey + 1));
                    
                    if (random.NextFloat() < GameConfig.CoinDropChance)
                    {
                        if (LocalTransformLookup.HasComponent(entity))
                        {
                            float3 pos = LocalTransformLookup[entity].Position;
                            Entity coin = ECB.Instantiate(sortKey, CoinConfig.CoinPrefab);
                            ECB.SetComponent(sortKey, coin, LocalTransform.FromPosition(pos));
                        }
                    }
                    ECB.DestroyEntity(sortKey, entity);
                }
            }
        }

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<CoinSpawnerConfig>();
            state.RequireForUpdate<GameConfigData>();
            state.RequireForUpdate<RandomComponent>();
            state.RequireForUpdate<SimulationSingleton>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            SimulationSingleton simulationSingleton = SystemAPI.GetSingleton<SimulationSingleton>();
            GameConfigData gameConfig = SystemAPI.GetSingleton<GameConfigData>();
            CoinSpawnerConfig coinConfig = SystemAPI.GetSingleton<CoinSpawnerConfig>();
            RefRW<RandomComponent> randomComponent = SystemAPI.GetSingletonRW<RandomComponent>();

            ComponentLookup<ProjectileTag> projectileLookup = SystemAPI.GetComponentLookup<ProjectileTag>(true);
            ComponentLookup<EnemyTag> enemyLookup = SystemAPI.GetComponentLookup<EnemyTag>(true);
            ComponentLookup<DamageData> damageLookup = SystemAPI.GetComponentLookup<DamageData>(true);
            ComponentLookup<LocalTransform> localTransformLookup = SystemAPI.GetComponentLookup<LocalTransform>(true);

            BeginSimulationEntityCommandBufferSystem.Singleton ecbSingleton = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>();
            EntityCommandBuffer.ParallelWriter ecb = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter();

            NativeParallelMultiHashMap<Entity, HitInfo> hitMap = new NativeParallelMultiHashMap<Entity, HitInfo>(128, Allocator.TempJob);

            ProjectileHitTriggerJob triggerJob = new ProjectileHitTriggerJob
            {
                ProjectileLookup = projectileLookup,
                EnemyLookup = enemyLookup,
                DamageLookup = damageLookup,
                HitMap = hitMap.AsParallelWriter()
            };

            JobHandle triggerHandle = triggerJob.Schedule(simulationSingleton, state.Dependency);

            ApplyDamageJob applyJob = new ApplyDamageJob
            {
                ECB = ecb,
                HitMap = hitMap,
                LocalTransformLookup = localTransformLookup,
                GameConfig = gameConfig,
                CoinConfig = coinConfig,
                Random = randomComponent.ValueRO
            };

            JobHandle applyHandle = applyJob.ScheduleParallel(triggerHandle);
            
            _ = hitMap.Dispose(applyHandle);

            randomComponent.ValueRW.Value.NextInt();

            state.Dependency = applyHandle;
        }
    }
}