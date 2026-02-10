using ArcadeShooter.Core;
using ArcadeShooter.Data;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Physics.Systems;
using Unity.Transforms;

namespace ArcadeShooter.Systems
{
    [BurstCompile]
    [UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
    [UpdateBefore(typeof(PhysicsSystemGroup))]
    public partial struct EnemyChasingSystem : ISystem
    {
        [BurstCompile]
        [WithAll(typeof(EnemyTag))]
        public partial struct EnemyChasingJob : IJobEntity
        {
            public float3 PlayerPosition;
            public float StopDistanceSq;

            public void Execute(ref PhysicsVelocity physicsVelocity, RefRO<LocalTransform> enemyTransform, RefRO<MoveSpeedData> moveSpeedData)
            {
                float3 enemyPosition = enemyTransform.ValueRO.Position;
                float3 direction = PlayerPosition - enemyPosition;
                float distanceSq = math.lengthsq(direction);

                if (distanceSq <= StopDistanceSq)
                {
                    physicsVelocity.Linear = new float3(0, physicsVelocity.Linear.y, 0);
                    return;
                }

                if (distanceSq > 0.01f)
                {
                    direction = math.normalize(direction);
                }
                
                float3 velocity = new float3(
                    direction.x * moveSpeedData.ValueRO.Value,
                    physicsVelocity.Linear.y,
                    direction.z * moveSpeedData.ValueRO.Value
                );
                physicsVelocity.Linear = velocity;
            }
        }

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<PlayerTag>();
            state.RequireForUpdate<GameConfigData>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            Entity playerEntity = SystemAPI.GetSingletonEntity<PlayerTag>();
            LocalTransform playerTransform = SystemAPI.GetComponent<LocalTransform>(playerEntity);
            float3 playerPosition = playerTransform.Position;

            GameConfigData gameConfig = SystemAPI.GetSingleton<GameConfigData>();
            float stopDistanceSq = gameConfig.StopDistance * gameConfig.StopDistance;

            new EnemyChasingJob
            {
                PlayerPosition = playerPosition,
                StopDistanceSq = stopDistanceSq
            }.ScheduleParallel();
        }
    }
}