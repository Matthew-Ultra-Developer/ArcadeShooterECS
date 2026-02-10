using ArcadeShooter.Data;
using Unity.Burst;
using Unity.Entities;

namespace ArcadeShooter.Systems
{
    [BurstCompile]
    [UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
    [UpdateAfter(typeof(EnemyDamageSystem))]
    public partial struct PlayerDeathCheckSystem : ISystem
    {
        [BurstCompile]
        [WithAll(typeof(PlayerTag))]
        [WithNone(typeof(DeadTag))]
        public partial struct PlayerDeathJob : IJobEntity
        {
            public EntityCommandBuffer.ParallelWriter ECB;

            public void Execute(Entity entity, [ChunkIndexInQuery] int chunkIndex, RefRO<HealthData> health)
            {
                if (health.ValueRO.Current <= 0f)
                {
                    ECB.AddComponent<DeadTag>(chunkIndex, entity);
                }
            }
        }

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<PlayerTag>();
            state.RequireForUpdate<BeginSimulationEntityCommandBufferSystem.Singleton>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            BeginSimulationEntityCommandBufferSystem.Singleton ecbSingleton = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>();
            EntityCommandBuffer ecb = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged);
            new PlayerDeathJob
            {
                ECB = ecb.AsParallelWriter()
            }.ScheduleParallel();
        }
    }
}