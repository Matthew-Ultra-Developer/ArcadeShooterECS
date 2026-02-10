using ArcadeShooter.Data;
using Unity.Burst;
using Unity.Entities;

namespace ArcadeShooter.UI
{
    [BurstCompile]
    public partial struct EnemyUISystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<EnemyTag>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            EndSimulationEntityCommandBufferSystem.Singleton cmdBufferSystem = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>();
            EntityCommandBuffer ecb = cmdBufferSystem.CreateCommandBuffer(state.WorldUnmanaged);

            foreach ((RefRO<EnemyTag> tag, Entity entity) in SystemAPI.Query<RefRO<EnemyTag>>().WithNone<EnemyUILinkData>().WithEntityAccess())
            {
                ecb.AddComponent<EnemyUILinkData>(entity);
            }
        }
    }
}
