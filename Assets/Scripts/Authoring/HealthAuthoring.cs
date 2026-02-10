using ArcadeShooter.Data;
using Unity.Entities;
using UnityEngine;

namespace ArcadeShooter.Authoring
{
    public class HealthAuthoring : MonoBehaviour
    {
        [SerializeField]
        private float currentHealth = 100f;

        [SerializeField]
        private float maxHealth = 100f;

        private class Baker : Baker<HealthAuthoring>
        {
            public override void Bake(HealthAuthoring authoring)
            {
                Entity entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity, new HealthData
                {
                    Current = authoring.currentHealth,
                    Max = authoring.maxHealth
                });
            }
        }
    }
}