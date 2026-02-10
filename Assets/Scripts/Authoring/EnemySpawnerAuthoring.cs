using ArcadeShooter.Data;
using Unity.Entities;
using UnityEngine;

namespace ArcadeShooter.Authoring
{
    public class EnemySpawnerAuthoring : MonoBehaviour
    {
        [SerializeField]
        private GameObject enemyPrefab;

        [SerializeField]
        private float spawnRate = 7f;

        private class Baker : Baker<EnemySpawnerAuthoring>
        {
            public override void Bake(EnemySpawnerAuthoring authoring)
            {
                Entity entity = GetEntity(TransformUsageFlags.None);
                AddComponent<GameSessionEntityTag>(entity);
                Entity prefabEntity = GetEntity(authoring.enemyPrefab, TransformUsageFlags.Dynamic);
                AddComponent(entity, new EnemySpawnerData
                {
                    PrefabEntity = prefabEntity,
                    SpawnRate = authoring.spawnRate,
                    Timer = authoring.spawnRate
                });
            }
        }
    }
}