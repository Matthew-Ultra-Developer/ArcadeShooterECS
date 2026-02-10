using ArcadeShooter.Data;
using Unity.Entities;
using UnityEngine;

namespace ArcadeShooter.Authoring
{
    public class DamageAuthoring : MonoBehaviour
    {
        [SerializeField]
        private float damageValue = 25f;

        private class Baker : Baker<DamageAuthoring>
        {
            public override void Bake(DamageAuthoring authoring)
            {
                Entity entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity, new DamageData
                {
                    Value = authoring.damageValue
                });
            }
        }
    }
}