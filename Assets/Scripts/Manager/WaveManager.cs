using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaveManager : MonoBehaviour
{

    public static WaveManager Instance { get; private set; }
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }
    [Header("Setup")]
    public List<Transform> spawnPoints;       // các điểm spawn
    public PlayerStats playerStats;

    [Header("Waves")]
    public List<EnemyWaveSO> waves;           // danh sách wave
    public int currentWaveIndex = 0;

    public static event Action OnEnemyKilled;       // event hiện tại
    public static event Action OneEnemyKilled;      // 🆕 Event mới cho mỗi enemy bị giết


    public ObservableValue<int> aliveEnemies = new(0); // số lượng enemy còn sống

    public static event Action<int> OnWaveStarted;   // int = wave index
    public static event Action OnWaveCleared;   // int = wave index
    public static event Action OnAllWavesCleared;



    private bool isSpawningWave = false;

    [Header("Game Stats")]
    public float gameStartTime;
    public int enemiesKilled = 0;

    // Properties for external access
    public float GameTime => Time.time - gameStartTime;
    public int EnemiesKilled => enemiesKilled;

    private int _currentWaveTotalEnemies = 0;
    private int _currentWaveKilledEnemies = 0; // 🆕 追加

    public int CurrentWaveTotalEnemies => _currentWaveTotalEnemies;
    public int CurrentWaveKilledEnemies => _currentWaveKilledEnemies;



    private void Start()
    {
        InitializeGame();
        StartNextWave(); // auto start wave đầu
    }

    /// <summary>
    /// Initialize game stats and start time
    /// </summary>
    private void InitializeGame()
    {
        gameStartTime = Time.time;
        enemiesKilled = 0;
        currentWaveIndex = 0;
        aliveEnemies.Value = 0;
        isSpawningWave = false;
    }

    public void StartNextWave()
    {
        if (isSpawningWave) return;
        if (currentWaveIndex >= waves.Count) return;

        EnemyWaveSO wave = waves[currentWaveIndex];
        StartCoroutine(SpawnWave(wave));
    }

    private IEnumerator WaitForWaveIntro()
    {
        float waitTime = 0f;
        bool finished = false;

        void OnWaveBannerFinished()
        {
            finished = true;
        }

        bool hasWaveUI = UIWaveCount.Instance != null && UIWaveCount.Instance.isActiveAndEnabled;

        UIWaveCount.OnWaveBannerFinished += OnWaveBannerFinished;

        if (hasWaveUI)
        {
            waitTime = UIWaveCount.Instance.GetAnimationDuration();
        }
        else
        {
            finished = true;
        }

        float elapsed = 0f;
        while (!finished && elapsed < waitTime + 0.5f)
        {
            elapsed += Time.deltaTime;
            yield return null;
        }

        UIWaveCount.OnWaveBannerFinished -= OnWaveBannerFinished;
    }

    private IEnumerator SpawnWave(EnemyWaveSO wave)
    {
        isSpawningWave = true;
        aliveEnemies.Value = 0;

        // 🆕 Waveごとのリセット
        _currentWaveKilledEnemies = 0;
        _currentWaveTotalEnemies = 0;

        // Wave内の総敵数をカウント
        foreach (var entry in wave.enemies)
        {
            _currentWaveTotalEnemies += entry.enemyCount;
        }

        OnWaveStarted?.Invoke(currentWaveIndex);
        yield return WaitForWaveIntro();
        Debug.Log($"Wave {currentWaveIndex + 1} started! (Total Enemies: {_currentWaveTotalEnemies})"); foreach (var entry in wave.enemies)
        {
            yield return new WaitForSeconds(entry.startDelay);

            for (int i = 0; i < entry.enemyCount; i++)
            {
                SpawnEnemy(entry);
                yield return new WaitForSeconds(entry.spawnRate);
            }
        }
        isSpawningWave = false;
    }
    private void SpawnEnemy(EnemyEntry entry)
    {
        if (spawnPoints.Count == 0) return;

        Transform spawnPoint = spawnPoints[UnityEngine.Random.Range(0, spawnPoints.Count)];
        EnemyBase enemyObj = Instantiate(entry.enemyData.enemyPrefab, spawnPoint.position, spawnPoint.rotation);
        enemyObj.Initialize(entry.enemyData, playerStats);

        aliveEnemies.Value++;
        enemyObj.OnDie += HandleEnemyDeath;

        // Register enemy on radar
        Radar.Instance?.RegisterEnemy(enemyObj.transform);
    }

    private void HandleEnemyDeath()
    {
        aliveEnemies.Value = Mathf.Max(0, aliveEnemies.Value - 1);
        enemiesKilled++; // 全体累計
        _currentWaveKilledEnemies++; // 🆕 Wave内カウント

        // 🆕 Gọi event mỗi khi 1 enemy bị giết
        OneEnemyKilled?.Invoke();

        if (aliveEnemies.Value <= 0 && !isSpawningWave)
        {
            OnWaveCleared?.Invoke();
            currentWaveIndex++;
            if (currentWaveIndex >= waves.Count)
            {
                Debug.Log("All waves completed!");
                OnAllWavesCleared?.Invoke();
            }
        }
    }


    /// <summary>
    /// Restart all waves (for game restart functionality)
    /// </summary>
    public void RestartWaves()
    {
        // Stop all coroutines
        StopAllCoroutines();

        // Clear all alive enemies
        ClearAllEnemies();

        // Reset game state
        InitializeGame();

        // Start first wave
        StartNextWave();

        Debug.Log("WaveManager: Game restarted!");
    }

    /// <summary>
    /// Clear all alive enemies from the scene
    /// </summary>
    private void ClearAllEnemies()
    {
        // Find all enemies in scene and destroy them
        EnemyBase[] allEnemies = FindObjectsOfType<EnemyBase>();
        foreach (var enemy in allEnemies)
        {
            if (enemy != null)
            {
                // Unregister from radar
                Radar.Instance?.UnregisterEnemy(enemy.transform);

                // Destroy enemy
                Destroy(enemy.gameObject);
            }
        }

        aliveEnemies.Value = 0;
    }


    /// <summary>
    /// Get current wave number (1-based)
    /// </summary>
    /// <returns>Current wave number</returns>
    public int GetCurrentWaveNumber()
    {
        return currentWaveIndex + 1;
    }

    /// <summary>
    /// Get total number of waves
    /// </summary>
    /// <returns>Total wave count</returns>
    public int GetTotalWaves()
    {
        return waves.Count;
    }

    /// <summary>
    /// Check if all waves are completed
    /// </summary>
    /// <returns>True if all waves are done</returns>
    public bool AreAllWavesCompleted()
    {
        return currentWaveIndex >= waves.Count;
    }

    /// <summary>
    /// Pause wave spawning (useful for game pause)
    /// </summary>
    public void PauseWaves()
    {
        StopAllCoroutines();
        isSpawningWave = false;
    }

    /// <summary>
    /// Resume wave spawning
    /// </summary>
    public void ResumeWaves()
    {
        if (currentWaveIndex < waves.Count && aliveEnemies.Value <= 0)
        {
            StartNextWave();
        }
    }

    /// <summary>
    /// 現在のWaveの合計敵数を取得
    /// </summary>
    public int GetCurrentWaveTotalEnemies()
    {
        return _currentWaveTotalEnemies;
    }

}