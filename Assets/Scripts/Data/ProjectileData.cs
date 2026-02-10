using Unity.Entities;
using Unity.Mathematics;

namespace ArcadeShooter.Data
{
    public struct ProjectileData : IComponentData
    {
        public float3 Direction;
        public float Speed;
    }
}