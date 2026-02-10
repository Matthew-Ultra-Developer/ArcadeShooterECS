using ArcadeShooter.Core;
using ArcadeShooter.Data;
using Unity.Entities;
using UnityEngine;

namespace ArcadeShooter.Authoring
{
    public class GameConfigAuthoring : MonoBehaviour
    {
        [SerializeField]
        private float enemyDamageRange = 1.5f;

        [SerializeField]
        private float projectileHitRadius = 0.5f;

        [SerializeField]
        private float coinCollectionRadius = 1.0f;

        [SerializeField]
        [Range(0f, 1f)]
        private float coinDropChance = 0.5f;

        [SerializeField]
        private float stopDistance = 2.0f;

        [SerializeField]
        private int maxEnemies = 100;

        [SerializeField]
        private uint randomSeed = 1;

        [Header("Spawn Settings")]
        [SerializeField]
        private float spawnDistanceMin = 15f;

        [SerializeField]
        private float spawnDistanceMax = 20f;

        [Header("Projectile Settings")]
        [SerializeField]
        private float defaultProjectileSpeed = 20f;

        private class Baker : Baker<GameConfigAuthoring>
        {
            public override void Bake(GameConfigAuthoring authoring)
            {
                Entity entity = GetEntity(TransformUsageFlags.None);
                AddComponent<GameSessionEntityTag>(entity);

                AddComponent(entity, new GameConfigData
                {
                    EnemyDamageRange = authoring.enemyDamageRange,
                    ProjectileHitRadius = authoring.projectileHitRadius,
                    CoinCollectionRadius = authoring.coinCollectionRadius,
                    CoinDropChance = authoring.coinDropChance,
                    StopDistance = authoring.stopDistance,
                    MaxEnemies = authoring.maxEnemies,
                    SpawnDistanceMin = authoring.spawnDistanceMin,
                    SpawnDistanceMax = authoring.spawnDistanceMax,
                    DefaultProjectileSpeed = authoring.defaultProjectileSpeed
                });

                AddComponent(entity, new RandomComponent
                {
                    Value = new Unity.Mathematics.Random(authoring.randomSeed)
                });
            }
        }
    }
}