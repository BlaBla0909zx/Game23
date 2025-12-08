using TMPro;
using UnityEngine;
public class UIEnemyCount : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI enemyCountText;
    private WaveManager waveManager;
    private void Start()
    {
        waveManager = WaveManager.Instance;
        if (enemyCountText == null || waveManager == null)
        {
            Debug.LogError("UIEnemyCount: Text hoặc WaveManager chưa gán!");
            return;
        }
        UpdateEnemyCountText();
    }
    private void OnEnable()
    {
        WaveManager.OneEnemyKilled += OnEnemyKilled;
        WaveManager.OnWaveCleared += OnWaveCleared;
        WaveManager.OnWaveStarted += OnWaveStarted;
    }
    private void OnDisable()
    {
        WaveManager.OneEnemyKilled -= OnEnemyKilled;
        WaveManager.OnWaveCleared -= OnWaveCleared;
        WaveManager.OnWaveStarted -= OnWaveStarted;
    }
    private void OnEnemyKilled() => UpdateEnemyCountText();
    private void OnWaveStarted(int waveIndex) => UpdateEnemyCountText();
    private void OnWaveCleared()
    {
        if (enemyCountText != null)
            enemyCountText.text = "Enemies Left: 0 / 0";
    }
    private void UpdateEnemyCountText()
    {
        if (waveManager == null || enemyCountText == null) return;
        int killed = waveManager.CurrentWaveKilledEnemies;
        int total = waveManager.GetCurrentWaveTotalEnemies();
        int remaining = Mathf.Max(total - killed, 0);
        enemyCountText.text = $"Enemies Left: {remaining} / {total}";
    }
}