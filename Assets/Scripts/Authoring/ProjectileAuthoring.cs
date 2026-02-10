using ArcadeShooter.Data;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace ArcadeShooter.Authoring
{
    public class ProjectileAuthoring : MonoBehaviour
    {
        [SerializeField]
        private float speed = 20f;

        private class Baker : Baker<ProjectileAuthoring>
        {
            public override void Bake(ProjectileAuthoring authoring)
            {
                Entity entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent<ProjectileTag>(entity);
                AddComponent<GameSessionEntityTag>(entity);

                AddComponent(entity, new ProjectileData
                {
                    Direction = float3.zero,
                    Speed = authoring.speed
                });
            }
        }
    }
}