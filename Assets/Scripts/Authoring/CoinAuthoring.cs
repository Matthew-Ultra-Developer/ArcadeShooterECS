using ArcadeShooter.Data;
using Unity.Entities;
using UnityEngine;

namespace ArcadeShooter.Authoring
{
    public class CoinAuthoring : MonoBehaviour
    {
        private class Baker : Baker<CoinAuthoring>
        {
            public override void Bake(CoinAuthoring authoring)
            {
                Entity entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent<CoinTag>(entity);
                AddComponent<GameSessionEntityTag>(entity);
            }
        }
    }
}