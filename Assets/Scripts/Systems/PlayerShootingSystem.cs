using ArcadeShooter.Core;
using ArcadeShooter.Data;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;

namespace ArcadeShooter.Systems
{
    [BurstCompile]
    public partial struct PlayerShootingSystem : ISystem
    {
        [BurstCompile]
        [WithAll(typeof(PlayerTag))]
        [WithNone(typeof(DeadTag))]
        public partial struct ShootingJob : IJobEntity
        {
            public float DeltaTime;
            public EntityCommandBuffer.ParallelWriter ECB;
            [ReadOnly] public PhysicsWorld PhysicsWorld;
            [ReadOnly] public ComponentLookup<EnemyTag> EnemyTagLookup;
            [ReadOnly] public ComponentLookup<ProjectileData> ProjectileLookup;
            [ReadOnly] public ComponentLookup<LocalTransform> TransformLookup;
            public float ProjectileHitRadius;
            public float DefaultProjectileSpeed;

            public void Execute(Entity entity, [ChunkIndexInQuery] int chunkIndex, RefRW<ShootingData> shootingData, RefRO<LocalTransform> playerTransform)
            {
                shootingData.ValueRW.Timer -= DeltaTime;
                if (shootingData.ValueRO.Timer <= 0f)
                {
                    shootingData.ValueRW.Timer = shootingData.ValueRO.FireRate;
                    float3 playerPosition = playerTransform.ValueRO.Position;

                    PointDistanceInput pointDistanceInput = new PointDistanceInput
                    {
                        Position = playerPosition,
                        MaxDistance = ProjectileHitRadius,
                        Filter = new CollisionFilter
                        {
                            BelongsTo = ~0u,
                            CollidesWith = Core.GameLayers.EnemyMask,
                            GroupIndex = 0
                        }
                    };

                    if (PhysicsWorld.CalculateDistance(pointDistanceInput, out DistanceHit hit))
                    {
                        if (EnemyTagLookup.HasComponent(hit.Entity))
                        {
                            float3 enemyPosition = TransformLookup[hit.Entity].Position;
                            float3 direction = math.normalize(enemyPosition - playerPosition);
                            
                            Entity projectileEntity = ECB.Instantiate(chunkIndex, shootingData.ValueRO.ProjectilePrefab);
                            LocalTransform projectileTransform = LocalTransform.FromPosition(playerPosition);
                            
                            if (TransformLookup.HasComponent(shootingData.ValueRO.ProjectilePrefab))
                            {
                                projectileTransform.Scale = TransformLookup[shootingData.ValueRO.ProjectilePrefab].Scale;
                            }
                            
                            ECB.SetComponent(chunkIndex, projectileEntity, projectileTransform);
                            
                            float speed = DefaultProjectileSpeed;
                            if (ProjectileLookup.HasComponent(shootingData.ValueRO.ProjectilePrefab))
                            {
                                speed = ProjectileLookup[shootingData.ValueRO.ProjectilePrefab].Speed;
                            }
                            
                            ProjectileData projectileData = new ProjectileData
                            {
                                Direction = direction,
                                Speed = speed
                            };
                            
                            ECB.SetComponent(chunkIndex, projectileEntity, projectileData);
                        }
                    }
                }
            }
        }

        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<PlayerTag>();
            state.RequireForUpdate<BeginSimulationEntityCommandBufferSystem.Singleton>();
            state.RequireForUpdate<PhysicsWorldSingleton>();
            state.RequireForUpdate<GameConfigData>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            float deltaTime = SystemAPI.Time.DeltaTime;
            BeginSimulationEntityCommandBufferSystem.Singleton ecbSingleton = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>();
            PhysicsWorldSingleton physicsWorldSingleton = SystemAPI.GetSingleton<PhysicsWorldSingleton>();
            
            ShootingJob shootingJob = new ShootingJob
            {
                DeltaTime = deltaTime,
                ECB = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter(),
                PhysicsWorld = physicsWorldSingleton.PhysicsWorld,
                EnemyTagLookup = SystemAPI.GetComponentLookup<EnemyTag>(true),
                ProjectileLookup = SystemAPI.GetComponentLookup<ProjectileData>(true),
                TransformLookup = SystemAPI.GetComponentLookup<LocalTransform>(true),
                ProjectileHitRadius = SystemAPI.GetSingleton<GameConfigData>().ProjectileHitRadius,
                DefaultProjectileSpeed = SystemAPI.GetSingleton<GameConfigData>().DefaultProjectileSpeed
            };
            state.Dependency = shootingJob.ScheduleParallel(state.Dependency);
        }
    }
}