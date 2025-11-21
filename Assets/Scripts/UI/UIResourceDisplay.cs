using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Global;
public class UIResourceDisplay : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI coinText;
    [SerializeField] private TextMeshProUGUI remainingEnemiesText;
    private int currentWave;
    private void Start()
    {
        Global.Utilities.WaitAfter(0.1f, () =>
        {
            PlayerStats.Instance.Coin.Subscribe(OnCoinChanged, true);
            WaveManager.Instance.aliveEnemies.Subscribe(OnAliveEnemiesChanged, true);
            WaveManager.OnWaveStarted += OnWaveStarted;
        });
    }
    private void OnWaveStarted(int waveIndex)
    {
        currentWave = waveIndex;
    }
    private void OnDestroy()
    {
        if (PlayerStats.Instance != null)
            PlayerStats.Instance.Coin.Unsubscribe(OnCoinChanged);
        if (WaveManager.Instance != null)
            WaveManager.Instance.aliveEnemies.Unsubscribe(OnAliveEnemiesChanged);
    }
    private void OnAliveEnemiesChanged(int oldvalue, int newvalue)
    {
        remainingEnemiesText.text = $"Wave <b><color=#FFD700>{currentWave}</color></b> Enemies Alive: <b><color=#FF5555>{newvalue}</color></b>";
    }
    private void OnCoinChanged(int oldvalue, int newCoinAmount)
    {
        // coinText.AnimateInt(newCoinAmount);
        coinText.text = $"Gold: {newCoinAmount}";
    }



}