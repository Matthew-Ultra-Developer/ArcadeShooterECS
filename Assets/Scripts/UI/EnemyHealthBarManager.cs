using System.Collections.Generic;
using ArcadeShooter.Data;
using TMPro;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.UI;

namespace ArcadeShooter.UI
{
    public class EnemyHealthBarManager : MonoBehaviour
    {
        private class HealthBarEntry
        {
            public GameObject GameObject;
            public TMP_Text HealthText;
            public Slider Slider;
            public float LastHealth;
            public float LastMaxHealth;
        }

        [SerializeField]
        private GameObject healthBarPrefab;

        [SerializeField]
        private Canvas worldSpaceCanvas;

        [SerializeField]
        private Vector3 healthBarOffset = new Vector3(0f, 1.5f, 0f);

        private EntityManager entityManager;
        private EntityQuery enemyQuery;
        private Dictionary<Entity, HealthBarEntry> activeHealthBars = new Dictionary<Entity, HealthBarEntry>();
        private Queue<HealthBarEntry> healthBarPool = new Queue<HealthBarEntry>();
        private List<Entity> entitiesToRemove = new List<Entity>();
        private HashSet<Entity> currentFrameEntities = new HashSet<Entity>();
        private bool initialized;
        private uint lastSystemVersion;

        private HealthBarEntry GetHealthBarFromPool()
        {
            HealthBarEntry entry;
            if (healthBarPool.Count > 0)
            {
                entry = healthBarPool.Dequeue();
            }
            else
            {
                GameObject obj = Instantiate(healthBarPrefab, worldSpaceCanvas.transform);
                entry = new HealthBarEntry
                {
                    GameObject = obj,
                    HealthText = obj.GetComponentInChildren<TMP_Text>(),
                    Slider = obj.GetComponentInChildren<Slider>(),
                    LastHealth = -1f,
                    LastMaxHealth = -1f
                };
            }
            entry.GameObject.SetActive(true);
            return entry;
        }

        private void ReturnHealthBarToPool(HealthBarEntry entry)
        {
            entry.GameObject.SetActive(false);
            healthBarPool.Enqueue(entry);
        }

        private void Start()
        {
            if (World.DefaultGameObjectInjectionWorld == null)
            {
                enabled = false;
                return;
            }
            entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;

            enemyQuery = entityManager.CreateEntityQuery(
                ComponentType.ReadOnly<EnemyTag>(),
                ComponentType.ReadOnly<HealthData>(),
                ComponentType.ReadOnly<LocalTransform>(),
                ComponentType.ReadOnly<EnemyUILinkData>()
            );

            initialized = true;
        }

        private void OnDestroy()
        {
            if (initialized && World.DefaultGameObjectInjectionWorld != null && World.DefaultGameObjectInjectionWorld.IsCreated)
            {
                enemyQuery.Dispose();
            }
        }

        private void LateUpdate()
        {
            if (!initialized || healthBarPrefab == null || worldSpaceCanvas == null)
            {
                return;
            }

            currentFrameEntities.Clear();

            EntityTypeHandle entityTypeHandle = entityManager.GetEntityTypeHandle();
            ComponentTypeHandle<LocalTransform> transformTypeHandle = entityManager.GetComponentTypeHandle<LocalTransform>(true);
            ComponentTypeHandle<HealthData> healthTypeHandle = entityManager.GetComponentTypeHandle<HealthData>(true);

            using (NativeArray<ArchetypeChunk> chunks = enemyQuery.ToArchetypeChunkArray(Allocator.Temp))
            {
                for (int i = 0; i < chunks.Length; i++)
                {
                    ArchetypeChunk chunk = chunks[i];
                    NativeArray<Entity> entities = chunk.GetNativeArray(entityTypeHandle);
                    NativeArray<LocalTransform> transforms = chunk.GetNativeArray(ref transformTypeHandle);
                    NativeArray<HealthData> healths = chunk.GetNativeArray(ref healthTypeHandle);

                    bool healthChanged = chunk.DidChange(ref healthTypeHandle, lastSystemVersion);

                    for (int j = 0; j < entities.Length; j++)
                    {
                        Entity entity = entities[j];
                        currentFrameEntities.Add(entity);

                        HealthBarEntry entry;
                        bool isNew = false;
                        if (!activeHealthBars.TryGetValue(entity, out entry))
                        {
                            entry = GetHealthBarFromPool();
                            activeHealthBars.Add(entity, entry);
                            isNew = true;
                        }

                        LocalTransform enemyTransform = transforms[j];
                        float3 worldPosition = enemyTransform.Position;
                        entry.GameObject.transform.position = new Vector3(worldPosition.x, worldPosition.y, worldPosition.z) + healthBarOffset;

                        if (healthChanged || isNew)
                        {
                            HealthData health = healths[j];
                            if (!Mathf.Approximately(health.Current, entry.LastHealth) || !Mathf.Approximately(health.Max, entry.LastMaxHealth))
                            {
                                entry.LastHealth = health.Current;
                                entry.LastMaxHealth = health.Max;

                                if (entry.HealthText != null)
                                {
                                    entry.HealthText.SetText("{0}/{1}", Mathf.CeilToInt(health.Current), Mathf.CeilToInt(health.Max));
                                }
                                if (entry.Slider != null)
                                {
                                    entry.Slider.value = health.Current / health.Max;
                                }
                            }
                        }
                    }
                }
            }

            entitiesToRemove.Clear();
            foreach (Entity entity in activeHealthBars.Keys)
            {
                if (!currentFrameEntities.Contains(entity))
                {
                    entitiesToRemove.Add(entity);
                }
            }

            foreach (Entity entity in entitiesToRemove)
            {
                ReturnHealthBarToPool(activeHealthBars[entity]);
                activeHealthBars.Remove(entity);
            }

            lastSystemVersion = entityManager.GlobalSystemVersion;
        }
    }
}
