using System.Collections.Generic;
using UnityEngine;

public class UIHealthBarController : MonoBehaviour
{
    public static UIHealthBarController Instance { get; private set; }
    [SerializeField] private UIHealthBar healthBarPrefab;
    [SerializeField] private int poolSize = 20;

    private List<HealthBarData> activeHealthBars = new List<HealthBarData>();
    private Queue<UIHealthBar> healthBarPool = new Queue<UIHealthBar>();
    private Canvas _canvas;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
        _canvas = GetComponent<Canvas>();
        InitializePool();
    }

    private void InitializePool()
    {
        for (int i = 0; i < poolSize; i++)
        {
            UIHealthBar healthBar = Instantiate(healthBarPrefab, transform);
            healthBar.gameObject.SetActive(false);
            healthBarPool.Enqueue(healthBar);
        }
    }

    private void Update()
    {
        UpdateHealthBarPositions();
    }


    private void UpdateHealthBarPositions()
    {
        for (int i = activeHealthBars.Count - 1; i >= 0; i--)
        {
            var healthBarData = activeHealthBars[i];

            // Check if target still exists
            if (healthBarData.target == null)
            {
                ReturnToPool(healthBarData.healthBar);
                activeHealthBars.RemoveAt(i);
                continue;
            }

            // Add safety checks for camera and position
            if (Camera.main == null)
            {
                Debug.LogWarning("Main camera not found!");
                continue;
            }

            Vector3 worldPos = healthBarData.target.position + Vector3.up * healthBarData.heightOffset;
            Vector3 screenPos = Camera.main.WorldToScreenPoint(worldPos);

            // Check if position is valid and in front of camera
            if (screenPos.z < 0)
            {
                // Target is behind camera, hide health bar
                healthBarData.healthBar.gameObject.SetActive(false);
                continue;
            }

            // Ensure health bar is visible
            healthBarData.healthBar.gameObject.SetActive(true);

            // Convert to canvas space if using Screen Space - Camera or World Space
            if (_canvas.renderMode == RenderMode.ScreenSpaceCamera || _canvas.renderMode == RenderMode.WorldSpace)
            {
                RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    _canvas.transform as RectTransform,
                    screenPos,
                    _canvas.worldCamera,
                    out Vector2 localPos);
                healthBarData.healthBar.transform.localPosition = localPos;
            }
            else
            {
                // Screen Space - Overlay
                healthBarData.healthBar.transform.position = screenPos;
            }
        }
    }
    public UIHealthBar RegisterHealthBar(Transform target, float maxHealth, float heightOffset = 2f)
    {
        UIHealthBar healthBar = GetFromPool();
        healthBar.SetHealth(maxHealth, maxHealth);
        Global.Utilities.WaitAfter(0.1f, () =>
        {
            healthBar.gameObject.SetActive(true);
        });

        activeHealthBars.Add(new HealthBarData { target = target, healthBar = healthBar, heightOffset = heightOffset });
        return healthBar;
    }

    public void UnregisterHealthBar(UIHealthBar healthBar)
    {
        for (int i = 0; i < activeHealthBars.Count; i++)
        {
            if (activeHealthBars[i].healthBar == healthBar)
            {
                ReturnToPool(healthBar);
                activeHealthBars.RemoveAt(i);
                break;
            }
        }
    }

    private UIHealthBar GetFromPool()
    {
        if (healthBarPool.Count > 0)
        {
            return healthBarPool.Dequeue();
        }
        else
        {
            // Pool is empty, create new one
            return Instantiate(healthBarPrefab, transform);
        }
    }

    private void ReturnToPool(UIHealthBar healthBar)
    {
        healthBar.gameObject.SetActive(false);
        healthBarPool.Enqueue(healthBar);
    }

    public void ClearAllHealthBars()
    {
        foreach (var healthBarData in activeHealthBars)
        {
            ReturnToPool(healthBarData.healthBar);
        }
        activeHealthBars.Clear();
    }
}
public struct HealthBarData
{
    public Transform target;
    public UIHealthBar healthBar;
    public float heightOffset;
}

public interface IHealthBar
{
    public void RegisterHealthBar(Transform target, float maxHealth, float heightOffset = 2f);
    public void SetHealth(float health, float maxHealth);
    public void UnregisterHealthBar();
}