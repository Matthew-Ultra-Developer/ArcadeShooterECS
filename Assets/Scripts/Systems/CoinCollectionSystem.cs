using ArcadeShooter.Data;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Physics;
using Unity.Physics.Systems;

namespace ArcadeShooter.Systems
{
    [BurstCompile]
    [UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
    [UpdateAfter(typeof(PhysicsSystemGroup))]
    public partial struct CoinCollectionSystem : ISystem
    {
        [BurstCompile]
        public struct CoinCollectionJob : ITriggerEventsJob
        {
            [ReadOnly]
            public ComponentLookup<CoinTag> CoinLookup;
            [ReadOnly]
            public ComponentLookup<PlayerTag> PlayerLookup;
            public EntityCommandBuffer.ParallelWriter ECB;

            public void Execute(TriggerEvent triggerEvent)
            {
                Entity entityA = triggerEvent.EntityA;
                Entity entityB = triggerEvent.EntityB;

                if (CoinLookup.HasComponent(entityA) && PlayerLookup.HasComponent(entityB))
                {
                    Collect(entityB, entityA, entityA.Index);
                }
                else if (CoinLookup.HasComponent(entityB) && PlayerLookup.HasComponent(entityA))
                {
                    Collect(entityA, entityB, entityB.Index);
                }
            }

            private void Collect(Entity playerEntity, Entity coinEntity, int sortKey)
            {
                ECB.DestroyEntity(sortKey, coinEntity);
                ECB.AppendToBuffer(sortKey, playerEntity, new CollectedCoinElement { Value = 1 });
            }
        }

        [BurstCompile]
        [WithAll(typeof(PlayerTag))]
        public partial struct ApplyStatsJob : IJobEntity
        {
            public void Execute(ref PlayerStatsData stats, ref DynamicBuffer<CollectedCoinElement> coins)
            {
                if (coins.IsEmpty)
                {
                    return;
                }

                int total = 0;
                foreach (CollectedCoinElement coin in coins)
                {
                    total += coin.Value;
                }

                stats.CoinsCollected += total;
                coins.Clear();
            }
        }

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<PlayerTag>();
            state.RequireForUpdate<SimulationSingleton>();
            state.RequireForUpdate<PhysicsWorldSingleton>();
            state.RequireForUpdate<BeginSimulationEntityCommandBufferSystem.Singleton>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            SimulationSingleton simulation = SystemAPI.GetSingleton<SimulationSingleton>();
            BeginSimulationEntityCommandBufferSystem.Singleton ecbSingleton = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>();
            EntityCommandBuffer ecb = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged);

            ComponentLookup<CoinTag> coinLookup = SystemAPI.GetComponentLookup<CoinTag>(true);
            ComponentLookup<PlayerTag> playerLookup = SystemAPI.GetComponentLookup<PlayerTag>(true);

            CoinCollectionJob coinJob = new CoinCollectionJob
            {
                CoinLookup = coinLookup,
                PlayerLookup = playerLookup,
                ECB = ecb.AsParallelWriter()
            };

            state.Dependency = coinJob.Schedule(simulation, state.Dependency);

            ApplyStatsJob applyJob = new ApplyStatsJob();
            state.Dependency = applyJob.Schedule(state.Dependency);
        }
    }
}