using Unity.Entities;

namespace ArcadeShooter.Data
{
    public struct PlayerStatsData : IComponentData
    {
        public int CoinsCollected;
    }
}