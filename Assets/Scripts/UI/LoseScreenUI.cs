using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEditor;
using Global;

public class LoseScreenUI : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField]
    private GameObject losePanel;

    [SerializeField]
    private Button playAgainButton;

    [SerializeField]
    private Button backToMenuButton;

    [Header("Animation Settings")]
    [SerializeField]
    private float fadeInDuration = 1f;

    [SerializeField]
    private Ease fadeEase = Ease.OutQuart;

    [Header("Audio")]
    [SerializeField]
    private string loseMusicName = "DefeatMusic";

    [SerializeField]
    private string buttonClickSFX = "ButtonClick";

    private CanvasGroup canvasGroup;
    private bool isVisible = false;

    private void Awake()
    {
        // Get or add CanvasGroup for fade effects
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }

        // Initially hide the panel
        losePanel.SetActive(false);
        canvasGroup.alpha = 0f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
    }

    private void Start()
    {
        // Setup button listeners
        if (playAgainButton != null)
            playAgainButton.onClick.AddListener(OnPlayAgainClicked);

        if (backToMenuButton != null)
            backToMenuButton.onClick.AddListener(OnBackToMenuClicked);
    }

    private void OnDestroy()
    {
        // Clean up button listeners
        if (playAgainButton != null)
            playAgainButton.onClick.RemoveListener(OnPlayAgainClicked);

        if (backToMenuButton != null)
            backToMenuButton.onClick.RemoveListener(OnBackToMenuClicked);
    }

    /// <summary>
    /// Show the lose screen with animation
    /// </summary>
    /// <param name="finalScore">Player's final score</param>
    /// <param name="survivalTime">Time survived</param>
    public void ShowLoseScreen(int finalScore = 0, float survivalTime = 0f)
    {
        if (isVisible)
            return;

        isVisible = true;

        losePanel.SetActive(true);

        // Play lose music
        PlayLoseMusic();

        // Animate in
        AnimateIn();
    }

    /// <summary>
    /// Hide the lose screen
    /// </summary>
    public void HideLoseScreen()
    {
        if (!isVisible)
            return;

        isVisible = false;
        AnimateOut();
    }

    /// <summary>
    /// Update the lose screen text with game statistics
    /// </summary>
    /// <param name="survivalTime">Time survived</param>
    private void UpdateLoseText(int finalScore, float survivalTime)
    {
        // Since there's no score system, we only show survival time
        // You can customize this to show other stats like enemies killed, waves completed, etc.
    }

    /// <summary>
    /// Animate the lose screen in
    /// </summary>
    private void AnimateIn()
    {
        Debug.Log("LoseScreenUI: Starting AnimateIn");
        
        // Show buttons immediately without animation
        if (playAgainButton != null)
        {
            playAgainButton.transform.localScale = Vector3.one;
            playAgainButton.interactable = true;
            playAgainButton.gameObject.SetActive(true);
            Debug.Log("LoseScreenUI: Play Again button shown immediately");
        }
        else
        {
            Debug.LogWarning("LoseScreenUI: Play Again button is null!");
        }

        if (backToMenuButton != null)
        {
            backToMenuButton.transform.localScale = Vector3.one;
            backToMenuButton.interactable = true;
            backToMenuButton.gameObject.SetActive(true);
            Debug.Log("LoseScreenUI: Back to Menu button shown immediately");
        }
        else
        {
            Debug.LogWarning("LoseScreenUI: Back to Menu button is null!");
        }

        // Fade in the panel (use unscaled time to work with timeScale = 0)
        canvasGroup.blocksRaycasts = true;
        Debug.Log("LoseScreenUI: Starting canvas fade in");
        canvasGroup
            .DOFade(1f, fadeInDuration)
            .SetEase(fadeEase)
            .SetUpdate(true) // Use unscaled time
            .OnComplete(() =>
            {
                Debug.Log("LoseScreenUI: Canvas fade complete, buttons are ready");
                canvasGroup.interactable = true;
            });
    }


    /// <summary>
    /// Animate the lose screen out
    /// </summary>
    private void AnimateOut()
    {
        canvasGroup.interactable = false;
        canvasGroup
            .DOFade(0f, fadeInDuration * 0.5f)
            .SetEase(fadeEase)
            .SetUpdate(true) // Use unscaled time
            .OnComplete(() =>
            {
                losePanel.SetActive(false);
                canvasGroup.blocksRaycasts = false;
            });
    }

    /// <summary>
    /// Play lose music
    /// </summary>
    private void PlayLoseMusic()
    {
        if (AudioManager.Instance != null && !string.IsNullOrEmpty(loseMusicName))
        {
            AudioManager.Instance.PlayMusic(loseMusicName, fadeIn: true, fadeTime: 2f);
        }
    }

    /// <summary>
    /// Play button click sound
    /// </summary>
    private void PlayButtonClickSound()
    {
        if (AudioManager.Instance != null && !string.IsNullOrEmpty(buttonClickSFX))
        {
            AudioManager.Instance.PlaySFX(buttonClickSFX, volumeMultiplier: 0.8f);
        }
    }

    #region Button Events

    /// <summary>
    /// Handle play again button click
    /// </summary>
    private void OnPlayAgainClicked()
    {
        PlayButtonClickSound();

        // Disable buttons to prevent multiple clicks
        if (playAgainButton != null)
            playAgainButton.interactable = false;
        if (backToMenuButton != null)
            backToMenuButton.interactable = false;

        // Stop lose music
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.StopMusic(fadeOut: true, fadeTime: 1f);
        }

        // Animate out and restart game
        AnimateOut();

        // Wait for animation to complete, then restart game (use unscaled time)
        DOVirtual.DelayedCall(
            fadeInDuration * 0.5f + 0.1f,
            () =>
            {
                RestartGame();
            }
        ).SetUpdate(true); // Use unscaled time
    }

    /// <summary>
    /// Handle back to menu button click
    /// </summary>
    private void OnBackToMenuClicked()
    {
        Loader.Load(Loader.Scene.Loading);
    }

    #endregion

    #region Scene Management

    /// <summary>
    /// Restart the game using GameOverManager
    /// </summary>
    private void RestartGame()
    {
        var gameOverManager = FindObjectOfType<GameOverManager>();
        if (gameOverManager != null)
        {
            gameOverManager.RestartGame();
        }
        else
        {
            // Fallback: reload scene
            ReloadGameScene();
        }
    }

    /// <summary>
    /// Reload the current game scene
    /// </summary>
    private void ReloadGameScene()
    {
        string currentSceneName = SceneManager.GetActiveScene().name;
        SceneManager.LoadScene(currentSceneName);
    }

    /// <summary>
    /// Load the main menu scene
    /// </summary>
    private void LoadMenuScene()
    {
        Loader.Load(Loader.Scene.Gameplay);
    }

    /// <summary>
    /// Check if a scene exists in the build settings
    /// </summary>
    /// <param name="sceneName">Name of the scene to check</param>
    /// <returns>True if scene exists</returns>
    private bool SceneExists(string sceneName)
    {
        for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
        {
            string scenePath = SceneUtility.GetScenePathByBuildIndex(i);
            string sceneNameFromPath = System.IO.Path.GetFileNameWithoutExtension(scenePath);

            if (sceneNameFromPath == sceneName)
            {
                return true;
            }
        }
        return false;
    }

    #endregion

    #region Public Methods for External Use

    /// <summary>
    /// Show lose screen with default values
    /// </summary>
    public void ShowLoseScreen()
    {
        ShowLoseScreen(0, 0f);
    }

    /// <summary>
    /// Show lose screen with score only
    /// </summary>
    /// <param name="finalScore">Final score</param>
    public void ShowLoseScreen(int finalScore)
    {
        ShowLoseScreen(finalScore, 0f);
    }

    /// <summary>
    /// Check if lose screen is currently visible
    /// </summary>
    /// <returns>True if visible</returns>
    public bool IsVisible()
    {
        return isVisible;
    }

    /// <summary>
    /// Force show buttons without animation (for debugging)
    /// </summary>
    [ContextMenu("Force Show Buttons")]
    public void ForceShowButtons()
    {
        Debug.Log("LoseScreenUI: Force showing buttons");
        
        if (playAgainButton != null)
        {
            playAgainButton.transform.localScale = Vector3.one;
            playAgainButton.interactable = true;
            playAgainButton.gameObject.SetActive(true);
            Debug.Log("LoseScreenUI: Play Again button forced visible");
        }
        else
        {
            Debug.LogWarning("LoseScreenUI: Play Again button is null!");
        }

        if (backToMenuButton != null)
        {
            backToMenuButton.transform.localScale = Vector3.one;
            backToMenuButton.interactable = true;
            backToMenuButton.gameObject.SetActive(true);
            Debug.Log("LoseScreenUI: Back to Menu button forced visible");
        }
        else
        {
            Debug.LogWarning("LoseScreenUI: Back to Menu button is null!");
        }
    }

    #endregion
}
