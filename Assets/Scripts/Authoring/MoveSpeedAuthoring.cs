using ArcadeShooter.Data;
using Unity.Entities;
using UnityEngine;

namespace ArcadeShooter.Authoring
{
    public class MoveSpeedAuthoring : MonoBehaviour
    {
        [SerializeField]
        private float moveSpeed = 5f;

        private class Baker : Baker<MoveSpeedAuthoring>
        {
            public override void Bake(MoveSpeedAuthoring authoring)
            {
                Entity entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity, new MoveSpeedData
                {
                    Value = authoring.moveSpeed
                });
            }
        }
    }
}