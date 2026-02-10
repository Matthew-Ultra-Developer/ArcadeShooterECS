using ArcadeShooter.Data;
using Unity.Entities;
using UnityEngine;

namespace ArcadeShooter.Authoring
{
    public class ShootingAuthoring : MonoBehaviour
    {
        [SerializeField]
        private GameObject projectilePrefab;

        [SerializeField]
        private float fireRate = 1f;

        private class Baker : Baker<ShootingAuthoring>
        {
            public override void Bake(ShootingAuthoring authoring)
            {
                Entity entity = GetEntity(TransformUsageFlags.Dynamic);
                Entity prefabEntity = GetEntity(authoring.projectilePrefab, TransformUsageFlags.Dynamic);
                AddComponent(entity, new ShootingData
                {
                    ProjectilePrefab = prefabEntity,
                    FireRate = authoring.fireRate,
                    Timer = 0f
                });
            }
        }
    }
}