using TMPro;
using UnityEngine;
using System.Collections;

public class UIEnemyCount : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI enemyCountText;
    private WaveManager waveManager;

    private IEnumerator Start()
    {
        // WaveManagerÇ™èâä˙âªÇ≥ÇÍÇÈÇ‹Ç≈ë“Ç¬
        yield return new WaitUntil(() => WaveManager.Instance != null);
        waveManager = WaveManager.Instance;

        if (enemyCountText == null)
        {
            Debug.LogError("UIEnemyCount: enemyCountText Ç™ñ¢ê›íËÇ≈Ç∑ÅB");
            yield break;
        }

        UpdateEnemyCountText();
    }

    private void OnEnable()
    {
        WaveManager.OnWaveCleared += OnWaveCleared;
    }

    private void OnDisable()
    {
        WaveManager.OnWaveCleared -= OnWaveCleared;
    }

    private void Update()
    {
        if (waveManager != null)
        {
            UpdateEnemyCountText();
        }
    }

    private void UpdateEnemyCountText()
    {
        if (waveManager == null || enemyCountText == null) return;

        int killed = waveManager.CurrentWaveKilledEnemies;
        int total = waveManager.GetCurrentWaveTotalEnemies();
        enemyCountText.text = $"Enemies Left: {killed} / {total}";
    }

    private void OnWaveCleared()
    {
        if (enemyCountText != null)
            enemyCountText.text = "Enemies Left: 0 / 0";
    }
}
