using Unity.Entities;

namespace ArcadeShooter.Data
{
    public struct EnemySpawnerData : IComponentData
    {
        public Entity PrefabEntity;
        public float SpawnRate;
        public float Timer;
    }
}