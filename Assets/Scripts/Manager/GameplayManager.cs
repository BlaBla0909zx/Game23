using UnityEngine;

public class GameplayManager : MonoBehaviour
{
    public static GameplayManager Instance { get; private set; }

    private UIShop _uiShop;
    private bool _isPaused = false;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        WaveManager.OnWaveCleared += OnWaveCleared;
        _uiShop = FindFirstObjectByType<UIShop>();
    }

    private void OnDestroy()
    {
        WaveManager.OnWaveCleared -= OnWaveCleared;
    }

    private void OnWaveCleared()
    {
        _uiShop.ActiveCanvas(true);
        PauseGame();

    }

    public void StartGame()
    {
        WaveManager.Instance.StartNextWave();
    }

    public void PauseGame()
    {
        _isPaused = true;
    }

    public void ResumeGame()
    {
        _isPaused = false;
        _uiShop.ActiveCanvas(false);
        WaveManager.Instance.StartNextWave();
    }
}
