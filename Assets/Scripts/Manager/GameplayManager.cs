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
        if (WaveManager.Instance != null &&
            WaveManager.Instance.currentWaveIndex >= WaveManager.Instance.waves.Count - 1)
        {
            // Final wave cleared: skip shop and allow game clear UI to appear instead
            return;
        }

        _uiShop.ActiveCanvas(true);
        PauseGame();

    }

    public void StartGame()
    {
        //Time.timeScale = 1f;
        WaveManager.Instance.StartNextWave();
    }

    public void PauseGame()
    {
        //Time.timeScale = 0f;
        _isPaused = true;
    }

    public void ResumeGame()
    {
        Time.timeScale = 1f;
        _isPaused = false;
        _uiShop.ActiveCanvas(false);
        WaveManager.Instance.StartNextWave();
    }
}
