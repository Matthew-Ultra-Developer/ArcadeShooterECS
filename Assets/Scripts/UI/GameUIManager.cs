using ArcadeShooter.Data;
using TMPro;
using Unity.Entities;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ArcadeShooter.UI
{
    public class GameUIManager : MonoBehaviour
    {
        public TMP_Text HealthText;
        public TMP_Text CoinText;
        public GameObject GameOverPanel;
        private bool gameOverShown = false;

        public void UpdateUI(float currentHealth, float maxHealth, int coins)
        {
            if (HealthText != null)
            {
                HealthText.SetText("HP: {0:0}/{1:0}", currentHealth, maxHealth);
            }
            if (CoinText != null)
            {
                CoinText.SetText("Coins: {0}", coins);
            }
        }

        public void ShowGameOver()
        {
            if ((!gameOverShown) && (GameOverPanel != null))
            {
                GameOverPanel.SetActive(true);
                gameOverShown = true;
                Time.timeScale = 0f;
            }
        }

        public void RestartGame()
        {
            Time.timeScale = 1f;
            if (World.DefaultGameObjectInjectionWorld != null)
            {
                EntityManager entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
                EntityQuery query = entityManager.CreateEntityQuery(typeof(GameSessionEntityTag));
                Unity.Collections.NativeArray<Entity> entities = query.ToEntityArray(Unity.Collections.Allocator.Temp);
                entityManager.DestroyEntity(entities);
                entities.Dispose();
            }
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        private void Start()
        {
            if (GameOverPanel != null)
            {
                GameOverPanel.SetActive(false);
            }
        }
    }
}