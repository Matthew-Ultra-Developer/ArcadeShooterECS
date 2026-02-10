using Unity.Entities;

namespace ArcadeShooter.Data
{
    public struct ShootingData : IComponentData
    {
        public Entity ProjectilePrefab;
        public float FireRate;
        public float Timer;
    }
}