using UnityEngine;
using UnityEngine.Pool;

public class ObjectSpawner : MonoBehaviour
{
    public static ObjectSpawner Instance { get; private set; }

    [SerializeField] private GameObject coinPrefab;
    [SerializeField] private int defaultCapacity = 10;
    [SerializeField] private int maxPoolSize = 100;

    private ObjectPool<GameObject> coinPool;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
            return;
        }
        Instance = this;

        // Create pool
        coinPool = new ObjectPool<GameObject>(
            CreateCoin,
            OnTakeFromPool,
            OnReturnedToPool,
            OnDestroyPoolObject,
            true,
            defaultCapacity,
            maxPoolSize
        );
    }

    private GameObject CreateCoin()
    {
        var coin = Instantiate(coinPrefab);
        coin.SetActive(false);
        return coin;
    }

    private void OnTakeFromPool(GameObject coin)
    {
        coin.SetActive(true);
        // RegisterCoin will be called in SpawnCoin method
    }

    private void OnReturnedToPool(GameObject coin)
    {
        coin.SetActive(false);
        CoinManager.Instance?.UnregisterCoin(coin);
    }

    private void OnDestroyPoolObject(GameObject coin)
    {
        Destroy(coin);
    }

    public void SpawnCoin(Vector3 position, int value)
    {
        var coin = coinPool.Get();
        coin.transform.position = position;
        CoinManager.Instance?.RegisterCoin(coin, value);
    }

    public void ReleaseCoin(GameObject coin)
    {
        coinPool.Release(coin);
    }


}