using ArcadeShooter.Data;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace ArcadeShooter.Authoring
{
    public class InputAuthoring : MonoBehaviour
    {
        private class Baker : Baker<InputAuthoring>
        {
            public override void Bake(InputAuthoring authoring)
            {
                Entity entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity, new InputData
                {
                    MoveDirection = float2.zero
                });
            }
        }
    }
}