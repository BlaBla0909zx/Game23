using TMPro;
using UnityEngine;
using System.Collections;

public class UIWaveCount : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI waveText;
    private WaveManager waveManager;

    private IEnumerator Start()
    {
        // WaveManagerが初期化されるまで待つ
        yield return new WaitUntil(() => WaveManager.Instance != null);
        waveManager = WaveManager.Instance;

        if (waveText == null)
        {
            Debug.LogError("UIWaveCount: waveText が未設定です。");
            yield break;
        }

        UpdateWaveTextSafe(waveManager.GetCurrentWaveNumber() - 1);
    }

    private void OnEnable()
    {
        WaveManager.OnWaveStarted += UpdateWaveTextSafe;
    }

    private void OnDisable()
    {
        WaveManager.OnWaveStarted -= UpdateWaveTextSafe;
    }

    private void UpdateWaveTextSafe(int waveIndex)
    {
        if (waveManager == null || waveText == null) return;

        int displayWave = waveIndex + 1;
        waveText.text = $"Wave: {displayWave} / {waveManager.GetTotalWaves()}";
    }
}
