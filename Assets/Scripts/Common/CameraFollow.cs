using ArcadeShooter.Data;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace ArcadeShooter.Common
{
    public class CameraFollow : MonoBehaviour
    {
        [SerializeField]
        private float followSpeed = 5f;
        [SerializeField]
        private Vector3 offset = new Vector3(0f, 10f, -10f);
        private EntityManager entityManager;
        private EntityQuery playerQuery;
        private Entity playerEntity;
        private bool playerFound;
        private bool initialized;

        private void FindPlayer()
        {
            if (playerQuery.CalculateEntityCount() > 0)
            {
                playerEntity = playerQuery.GetSingletonEntity();
                playerFound = true;
            }
        }

        private void Start()
        {
            if (World.DefaultGameObjectInjectionWorld == null)
            {
                enabled = false;
                return;
            }
            entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
            playerQuery = entityManager.CreateEntityQuery(typeof(PlayerTag));
            initialized = true;
        }

        private void OnDestroy()
        {
            if ((initialized) && (World.DefaultGameObjectInjectionWorld != null) && (World.DefaultGameObjectInjectionWorld.IsCreated))
            {
                playerQuery.Dispose();
            }
        }

        private void LateUpdate()
        {
            if ((!initialized) || (World.DefaultGameObjectInjectionWorld == null) || (!World.DefaultGameObjectInjectionWorld.IsCreated))
            {
                return;
            }
            if (!playerFound)
            {
                FindPlayer();
                return;
            }
            if (!entityManager.Exists(playerEntity))
            {
                playerFound = false;
                return;
            }
            LocalTransform playerTransform = entityManager.GetComponentData<LocalTransform>(playerEntity);
            float3 playerPosition = playerTransform.Position;
            Vector3 targetPosition = new Vector3(playerPosition.x, playerPosition.y, playerPosition.z) + offset;
            transform.position = Vector3.Lerp(transform.position, targetPosition, followSpeed * Time.deltaTime);
        }
    }
}