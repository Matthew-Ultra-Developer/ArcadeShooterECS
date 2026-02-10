using ArcadeShooter.Data;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Physics.Systems;

namespace ArcadeShooter.Systems
{
    [BurstCompile]
    [UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
    [UpdateBefore(typeof(PhysicsSystemGroup))]
    public partial struct PlayerMoveSystem : ISystem
    {
        [BurstCompile]
        [WithAll(typeof(PlayerTag))]
        [WithNone(typeof(DeadTag))]
        public partial struct PlayerMoveJob : IJobEntity
        {
            public void Execute(ref PhysicsVelocity physicsVelocity, RefRO<InputData> inputData, RefRO<MoveSpeedData> moveSpeedData)
            {
                float3 currentVelocity = physicsVelocity.Linear;
                float3 targetVelocity = new float3(
                    inputData.ValueRO.MoveDirection.x * moveSpeedData.ValueRO.Value,
                    currentVelocity.y,
                    inputData.ValueRO.MoveDirection.y * moveSpeedData.ValueRO.Value
                );
                physicsVelocity.Linear = targetVelocity;
            }
        }

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<PlayerTag>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            new PlayerMoveJob().ScheduleParallel();
        }
    }
}