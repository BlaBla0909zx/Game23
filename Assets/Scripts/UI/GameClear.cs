using Global;
using UnityEngine;
using UnityEngine.UI;

public class GameClear : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField]
    private GameObject clearPanel;

    [SerializeField]
    private Button backToMenuButton;

    [Header("Audio")]
    [SerializeField]
    private string buttonClickSFX = "ButtonClick";
    
    [SerializeField] private CanvasGroup canvasGroup;

    private bool isVisible = false;


    private void Awake()
    {
        WaveManager.OnAllWavesCleared += HandleGameCleared;

        if (backToMenuButton != null)
        {
            backToMenuButton.onClick.AddListener(OnBackToMenuClicked);
        }

        if (clearPanel != null)
        {
            clearPanel.SetActive(false);
        }
        // Get or add CanvasGroup for fade effects
        //canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup != null)
            {
               canvasGroup.alpha = 0f;
               canvasGroup.interactable = true;
               canvasGroup.blocksRaycasts = true;
            }
        
    }

    private void OnDestroy()
    {
        WaveManager.OnAllWavesCleared -= HandleGameCleared;

        if (backToMenuButton != null)
        {
            backToMenuButton.onClick.RemoveListener(OnBackToMenuClicked);
        }
    }

    private void HandleGameCleared()
    {
        if (isVisible)
        {
            return;
        }

        isVisible = true;

        if (clearPanel != null)
        {
            clearPanel.SetActive(true);
        }
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 1f;
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
        }
    }

    private void OnBackToMenuClicked()
    {
        PlayButtonSound();
        Time.timeScale = 1f;
        Loader.Load(Loader.Scene.Loading);
    }

    private void PlayButtonSound()
    {
        if (AudioManager.Instance != null && !string.IsNullOrEmpty(buttonClickSFX))
        {
            AudioManager.Instance.PlaySFX(buttonClickSFX, volumeMultiplier: 0.8f);
        }
    }
}