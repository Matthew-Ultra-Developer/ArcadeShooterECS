using ArcadeShooter.Data;
using Unity.Entities;
using UnityEngine;

namespace ArcadeShooter.UI
{
    public partial class UIBridgeSystem : SystemBase
    {
        private GameUIManager gameUIManager;
        private EntityQuery playerQuery;
        private bool initialized;
        private float lastHealth;
        private int lastCoins;

        protected override void OnCreate()
        {
            RequireForUpdate<PlayerTag>();
            playerQuery = GetEntityQuery(typeof(PlayerTag));
        }

        protected override void OnStartRunning()
        {
            lastHealth = -1f;
            lastCoins = -1;
        }

        protected override void OnUpdate()
        {
            if (gameUIManager == null)
            {
                gameUIManager = Object.FindFirstObjectByType<GameUIManager>();
                initialized = gameUIManager != null;
                if (initialized)
                {
                    lastHealth = -1f;
                    lastCoins = -1;
                }
                else
                {
                    return;
                }
            }
            Entity playerEntity = SystemAPI.GetSingletonEntity<PlayerTag>();
            HealthData healthData = SystemAPI.GetComponent<HealthData>(playerEntity);
            PlayerStatsData playerStatsData = SystemAPI.GetComponent<PlayerStatsData>(playerEntity);
            if (!Mathf.Approximately(healthData.Current, lastHealth))
            {
                gameUIManager.UpdateUI(healthData.Current, healthData.Max, playerStatsData.CoinsCollected);
                lastHealth = healthData.Current;
            }
            if (playerStatsData.CoinsCollected != lastCoins)
            {
                gameUIManager.UpdateUI(healthData.Current, healthData.Max, playerStatsData.CoinsCollected);
                lastCoins = playerStatsData.CoinsCollected;
            }
            if (healthData.Current <= 0f)
            {
                gameUIManager.ShowGameOver();
            }
        }
    }
}