using Unity.Entities;

namespace ArcadeShooter.Core
{
    public struct GameConfigData : IComponentData
    {
        public float EnemyDamageRange;
        public float ProjectileHitRadius;
        public float CoinCollectionRadius;
        public float CoinDropChance;
        public float StopDistance;
        public int MaxEnemies;
        public float SpawnDistanceMin;
        public float SpawnDistanceMax;
        public float DefaultProjectileSpeed;
    }
}