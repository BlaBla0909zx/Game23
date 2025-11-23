using TMPro;
using UnityEngine;

/// <summary>
/// Simple manager that handles game over when player dies
/// Listens to PlayerStats death event and manages game state
/// </summary>
public class GameOverManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField]
    private PlayerStats playerStats;

    [SerializeField]
    private LoseScreenUI loseScreenUI;



    [Header("Game Over Settings")]
    [SerializeField]
    private float gameOverDelay = 1f;

    [SerializeField]
    private bool pauseGameOnDeath = true;

    [Header("Debug")]
    [SerializeField]
    private bool debugMode = false;

    private bool isGameOver = false;

    private void Awake()
    {

    }

    private void Start()
    {
        // Subscribe to player death event
        if (playerStats != null)
        {
            playerStats.OnPlayerDeath += OnPlayerDied;
            if (debugMode)
                Debug.Log("GameOverManager: Subscribed to player death event");
        }
        else
        {
            Debug.LogError("GameOverManager: PlayerStats not found!");
        }
    }

    private void OnDestroy()
    {
        // Unsubscribe from events
        if (playerStats != null)
        {
            playerStats.OnPlayerDeath -= OnPlayerDied;
        }

        // Ensure time scale is reset
        if (Time.timeScale == 0f)
        {
            Time.timeScale = 1f;
        }
    }

    /// <summary>
    /// Handle player death event
    /// </summary>
    private void OnPlayerDied()
    {
        if (isGameOver)
            return;

        isGameOver = true;

        if (debugMode)
            Debug.Log("GameOverManager: Player died, triggering game over...");

        // Delay game over to allow for death animations
        Invoke(nameof(TriggerGameOver), gameOverDelay);
    }

    /// <summary>
    /// Trigger the game over sequence
    /// </summary>
    private void TriggerGameOver()
    {
        if (debugMode)
            Debug.Log("GameOverManager: Triggering game over sequence...");

        // Pause the game
        if (pauseGameOnDeath)
        {
            PauseGame();
        }

    }

    /// <summary>
    /// Pause the game (set time scale to 0)
    /// </summary>
    public void PauseGame()
    {
        Time.timeScale = 0f;
        Debug.Log("GameOverManager: Game paused (time scale = 0)");
    }

    /// <summary>
    /// Resume the game (set time scale to 1)
    /// </summary>
    public void ResumeGame()
    {
        Time.timeScale = 1f;
        if (debugMode)
            Debug.Log("GameOverManager: Game resumed (time scale = 1)");
    }

    /// <summary>
    /// Restart the game using wave manager
    /// </summary>
    public void RestartGame()
    {
        if (debugMode)
            Debug.Log("GameOverManager: Restarting game...");

        // Resume game time
        ResumeGame();

        // Reset game over state
        isGameOver = false;

        // Restart using wave manager
        if (WaveManager.Instance != null)
        {
            WaveManager.Instance.RestartWaves();
        }
        else
        {
            Debug.LogWarning("GameOverManager: WaveManager not found! Reloading scene instead.");
            ReloadCurrentScene();
        }

        // Reset player health
        if (playerStats != null)
        {
            playerStats.ResetHealth();
        }
    }


    /// <summary>
    /// Get survival time from game systems
    /// </summary>


    /// <summary>
    /// Reload the current scene as fallback
    /// </summary>
    private void ReloadCurrentScene()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene().name
        );
    }

    /// <summary>
    /// Check if game is currently over
    /// </summary>
    /// <returns>True if game is over</returns>
    public bool IsGameOver()
    {
        return isGameOver;
    }

    /// <summary>
    /// Force trigger game over (for testing)
    /// </summary>
    [ContextMenu("Force Game Over")]
    public void ForceGameOver()
    {
        if (!isGameOver && playerStats != null)
        {
            // Force player to die by dealing enough damage
            float damageAmount = playerStats.GetCurrentHealth() + 1;
            DamageResult damageResult = new DamageResult
            {
                FinalDamage = damageAmount,
                IsCrit = false,
                IsMiss = false,
                IsLifeSteal = false
            };
            playerStats.TakeDamage(damageResult, Vector3.zero);
        }
    }

    /// <summary>
    /// Force restart game (for testing)
    /// </summary>
    [ContextMenu("Force Restart")]
    public void ForceRestart()
    {
        RestartGame();
    }
}
