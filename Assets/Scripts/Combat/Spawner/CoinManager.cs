using System.Collections.Generic;
using UnityEngine;

public class CoinManager : MonoBehaviour
{
    public static CoinManager Instance { get; private set; }

    [Header("Magnet Settings")]
    [SerializeField] private float magnetDelay = 2f;
    [SerializeField] private float magnetSpeed = 10f;
    [SerializeField] private float pickupDistance = 1f;
    [SerializeField] private int maxActiveCoins = 50;

    private Transform player;
    private readonly List<CoinData> activeCoins = new List<CoinData>();

    private struct CoinData
    {
        public GameObject coinObject;
        public float spawnTime;
        public bool isMagnetActive;
        public int value;
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        player = GameObject.FindWithTag("Player")?.transform;
    }

    private void Update()
    {
        if (player == null) return;

        float currentTime = Time.time;

        for (int i = activeCoins.Count - 1; i >= 0; i--)
        {
            var coinData = activeCoins[i];

            // Check if coin object is null (destroyed)
            if (coinData.coinObject == null)
            {
                activeCoins.RemoveAt(i);
                continue;
            }

            // Check if magnet should activate
            if (!coinData.isMagnetActive && currentTime - coinData.spawnTime >= magnetDelay)
            {
                coinData.isMagnetActive = true;
                activeCoins[i] = coinData;
            }

            if (coinData.isMagnetActive)
            {
                Vector3 coinPos = coinData.coinObject.transform.position;
                Vector3 playerPos = player.position;

                // Move towards player
                coinData.coinObject.transform.position = Vector3.MoveTowards(
                    coinPos, playerPos, magnetSpeed * Time.deltaTime);

                // Check pickup distance
                if (Vector3.Distance(coinPos, playerPos) <= pickupDistance)
                {
                    PlayerStats.Instance.Coin.Value += coinData.value;
                    activeCoins.RemoveAt(i);
                    ObjectSpawner.Instance.ReleaseCoin(coinData.coinObject);
                }
            }
        }
    }

    public void RegisterCoin(GameObject coin, int value)
    {
        if (activeCoins.Count >= maxActiveCoins)
            return;

        // Check if coin is already registered to avoid duplicates
        for (int i = 0; i < activeCoins.Count; i++)
        {
            if (activeCoins[i].coinObject == coin)
                return; // Already registered
        }

        activeCoins.Add(new CoinData
        {
            coinObject = coin,
            spawnTime = Time.time,
            isMagnetActive = false,
            value = value
        });
    }

    public void UnregisterCoin(GameObject coin)
    {
        for (int i = activeCoins.Count - 1; i >= 0; i--)
        {
            if (activeCoins[i].coinObject == coin)
            {
                activeCoins.RemoveAt(i);
                break;
            }
        }
    }
}

