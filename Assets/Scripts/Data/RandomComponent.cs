using Unity.Entities;

namespace ArcadeShooter.Data
{
    public struct RandomComponent : IComponentData
    {
        public Unity.Mathematics.Random Value;
    }
}
