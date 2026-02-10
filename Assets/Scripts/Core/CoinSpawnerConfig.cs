using Unity.Entities;

namespace ArcadeShooter.Core
{
    public struct CoinSpawnerConfig : IComponentData
    {
        public Entity CoinPrefab;
    }
}