using ArcadeShooter.Core;
using ArcadeShooter.Data;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace ArcadeShooter.Systems
{
    [BurstCompile]
    public partial struct EnemySpawnerSystem : ISystem
    {
        [BurstCompile]
        public partial struct EnemySpawnerJob : IJobEntity
        {
            public float DeltaTime;
            public EntityCommandBuffer ECB;
            public float3 PlayerPosition;
            public ComponentLookup<RandomComponent> RandomLookup;
            public Entity RandomEntity;
            public GameConfigData GameConfig;
            public bool ShouldSpawn;

            public void Execute(RefRW<EnemySpawnerData> spawnerData)
            {
                spawnerData.ValueRW.Timer -= DeltaTime;
                if (spawnerData.ValueRO.Timer <= 0f)
                {
                    spawnerData.ValueRW.Timer = spawnerData.ValueRO.SpawnRate;
                    if (ShouldSpawn)
                    {
                        RefRW<RandomComponent> randomComponent = RandomLookup.GetRefRW(RandomEntity);
                        float angle = randomComponent.ValueRW.Value.NextFloat(0f, math.PI * 2f);
                        float distance = randomComponent.ValueRW.Value.NextFloat(GameConfig.SpawnDistanceMin, GameConfig.SpawnDistanceMax);
                        float3 spawnOffset = new float3(
                            math.cos(angle) * distance,
                            0f,
                            math.sin(angle) * distance
                        );
                        float3 spawnPosition = PlayerPosition + spawnOffset;
                        Entity enemyEntity = ECB.Instantiate(spawnerData.ValueRO.PrefabEntity);
                        LocalTransform newTransform = LocalTransform.FromPosition(spawnPosition);
                        newTransform.Scale = 1f;
                        ECB.SetComponent(enemyEntity, newTransform);
                    }
                }
            }
        }

        private EntityQuery enemyQuery;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<PlayerTag>();
            state.RequireForUpdate<GameConfigData>();
            state.RequireForUpdate<RandomComponent>();
            state.RequireForUpdate<BeginSimulationEntityCommandBufferSystem.Singleton>();
            enemyQuery = state.GetEntityQuery(ComponentType.ReadOnly<EnemyTag>());
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            float deltaTime = SystemAPI.Time.DeltaTime;
            BeginSimulationEntityCommandBufferSystem.Singleton ecbSingleton = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>();
            GameConfigData config = SystemAPI.GetSingleton<GameConfigData>();
            Entity playerEntity = SystemAPI.GetSingletonEntity<PlayerTag>();
            LocalTransform playerTransform = SystemAPI.GetComponent<LocalTransform>(playerEntity);
            int enemyCount = enemyQuery.CalculateEntityCount();
            bool shouldSpawn = enemyCount < config.MaxEnemies;
            
            Entity randomEntity = SystemAPI.GetSingletonEntity<RandomComponent>();
            ComponentLookup<RandomComponent> randomLookup = SystemAPI.GetComponentLookup<RandomComponent>(false);

            new EnemySpawnerJob
            {
                DeltaTime = deltaTime,
                ECB = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged),
                PlayerPosition = playerTransform.Position,
                RandomLookup = randomLookup,
                RandomEntity = randomEntity,
                GameConfig = config,
                ShouldSpawn = shouldSpawn
            }.Schedule();
        }
    }
}
