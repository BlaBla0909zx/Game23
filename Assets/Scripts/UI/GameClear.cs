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
