using Unity.Entities;
using Unity.Mathematics;

namespace ArcadeShooter.Data
{
    public struct InputData : IComponentData
    {
        public float2 MoveDirection;
    }
}