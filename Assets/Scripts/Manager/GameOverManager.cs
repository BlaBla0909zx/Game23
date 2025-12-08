using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

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

    [Header("Audio")]
    [SerializeField]
    private string gameplayMusicName = "bg";



    [Header("Game Over Settings")]
    [SerializeField]
    private float gameOverDelay = 1f;

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

        // Ensure base gameplay music is active whenever the gameplay scene loads
        ResumeGameplayMusic();
    }

    private void OnDestroy()
    {
        // Unsubscribe from events
        if (playerStats != null)
        {
            playerStats.OnPlayerDeath -= OnPlayerDied;
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

        // Stop current gameplay music before switching to defeat audio
        StopGameplayMusic();

        // Show lose screen without altering time scale
        if (loseScreenUI != null)
        {
            loseScreenUI.ShowLoseScreen();
        }
        else if (debugMode)
        {
            Debug.LogWarning("GameOverManager: LoseScreenUI reference missing");
        }

    }

    /// <summary>
    /// Pause the game (legacy placeholder, time scale no longer adjusted)
    /// </summary>
    public void PauseGame()
    {
        Debug.Log("GameOverManager: Pause requested (time scale unchanged)");
    }

    /// <summary>
    /// Resume the game (legacy placeholder, time scale no longer adjusted)
    /// </summary>
    public void ResumeGame()
    {
        if (debugMode)
            Debug.Log("GameOverManager: Game resumed (time scale unchanged)");
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

        // Hide lose screen state so it can show again after the restart
        if (loseScreenUI != null)
        {
            loseScreenUI.HideLoseScreen();
        }

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

        // Resume gameplay music after stopping defeat track
        ResumeGameplayMusic();
    }


    /// <summary>
    /// Get survival time from game systems
    /// </summary>


    /// <summary>
    /// Reload the current scene as fallback
    /// </summary>
    public void ReloadCurrentScene()
    {
        Scene currentScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(currentScene.name);
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

    /// <summary>
    /// Reset audio/UI state when returning to the menu so the next run has music
    /// </summary>
    public void PrepareReturnToMenu()
    {
        // Clear any defeat music so the menu/loading scenes can start their own audio
        StopGameplayMusic();

        // Hide lose screen to avoid lingering overlays if the gameplay scene reloads
        if (loseScreenUI != null)
        {
            loseScreenUI.HideLoseScreen();
        }

        isGameOver = false;
    }

    private void StopGameplayMusic()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.StopMusic(fadeOut: true, fadeTime: 0.5f);
        }
    }

    private void ResumeGameplayMusic()
    {
        if (
            AudioManager.Instance != null
            && !string.IsNullOrEmpty(gameplayMusicName)
        )
        {
            AudioManager.Instance.PlayMusic(gameplayMusicName, fadeIn: true, fadeTime: 1f);
        }
    }
}
