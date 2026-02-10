using ArcadeShooter.Data;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace ArcadeShooter.Systems
{
    [BurstCompile]
    public partial struct PlayerInputSystem : ISystem
    {
        [BurstCompile]
        [WithAll(typeof(PlayerTag))]
        public partial struct ApplyInputJob : IJobEntity
        {
            public float2 MoveDirection;

            public void Execute(RefRW<InputData> inputData)
            {
                inputData.ValueRW.MoveDirection = MoveDirection;
            }
        }

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<PlayerTag>();
        }

        public void OnUpdate(ref SystemState state)
        {
            float horizontal = 0f;
            float vertical = 0f;
            if (Input.GetKey(KeyCode.W))
            {
                vertical += 1f;
            }
            if (Input.GetKey(KeyCode.S))
            {
                vertical -= 1f;
            }
            if (Input.GetKey(KeyCode.A))
            {
                horizontal -= 1f;
            }
            if (Input.GetKey(KeyCode.D))
            {
                horizontal += 1f;
            }
            float2 moveDirection = new float2(horizontal, vertical);
            if (math.lengthsq(moveDirection) > 0f)
            {
                moveDirection = math.normalize(moveDirection);
            }
            new ApplyInputJob
            {
                MoveDirection = moveDirection
            }.ScheduleParallel();
        }
    }
}