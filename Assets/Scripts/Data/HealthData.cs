using Unity.Entities;

namespace ArcadeShooter.Data
{
    public struct HealthData : IComponentData
    {
        public float Current;
        public float Max;
    }
}