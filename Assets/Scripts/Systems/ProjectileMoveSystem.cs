using ArcadeShooter.Data;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;

namespace ArcadeShooter.Systems
{
    [BurstCompile]
    public partial struct ProjectileMoveSystem : ISystem
    {
        [BurstCompile]
        [WithAll(typeof(ProjectileTag))]
        public partial struct ProjectileMoveJob : IJobEntity
        {
            public void Execute(ref PhysicsVelocity velocity, RefRO<ProjectileData> projectileData)
            {
                velocity.Linear = projectileData.ValueRO.Direction * projectileData.ValueRO.Speed;
                velocity.Angular = float3.zero;
            }
        }

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<ProjectileTag>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            new ProjectileMoveJob().ScheduleParallel();
        }
    }
}