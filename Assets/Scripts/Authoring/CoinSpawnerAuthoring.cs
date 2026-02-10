using ArcadeShooter.Core;
using Unity.Entities;
using UnityEngine;

namespace ArcadeShooter.Authoring
{
    public class CoinSpawnerAuthoring : MonoBehaviour
    {
        [SerializeField]
        private GameObject coinPrefab;

        private class Baker : Baker<CoinSpawnerAuthoring>
        {
            public override void Bake(CoinSpawnerAuthoring authoring)
            {
                Entity entity = GetEntity(TransformUsageFlags.None);
                Entity prefabEntity = GetEntity(authoring.coinPrefab, TransformUsageFlags.Dynamic);
                AddComponent(entity, new CoinSpawnerConfig
                {
                    CoinPrefab = prefabEntity
                });
            }
        }
    }
}