using ArcadeShooter.Data;
using Unity.Entities;
using UnityEngine;

namespace ArcadeShooter.Authoring
{
    public class PlayerStatsAuthoring : MonoBehaviour
    {
        private class Baker : Baker<PlayerStatsAuthoring>
        {
            public override void Bake(PlayerStatsAuthoring authoring)
            {
                Entity entity = GetEntity(TransformUsageFlags.Dynamic);

                AddComponent(entity, new PlayerStatsData
                {
                    CoinsCollected = 0
                });
            }
        }
    }
}