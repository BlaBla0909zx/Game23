using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class Radar : MonoBehaviour
{
    public static Radar Instance { get; private set; }

    [Header("Radar Settings")]
    public float radarRange = 50f;
    public RectTransform radarRect;
    public GameObject enemyIconPrefab;
    public GameObject playerIconPrefab;

    [Header("Player")]
    private Transform player;
    private GameObject playerIcon;

    [Header("Enemies")]
    private List<Transform> registeredEnemies = new List<Transform>();
    private List<GameObject> enemyIcons = new List<GameObject>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void Start()
    {
        // Auto-register player if PlayerStats exists
        if (PlayerStats.Instance != null)
        {
            RegisterPlayer(PlayerStats.Instance.transform);
        }
    }

    private void Update()
    {
        UpdateRadar();
    }

    public void RegisterPlayer(Transform playerTransform)
    {
        player = playerTransform;

        if (playerIconPrefab != null && radarRect != null)
        {
            // Create player icon at center
            playerIcon = Instantiate(playerIconPrefab, radarRect);
            playerIcon.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
            Debug.Log("Player registered on radar");
        }
    }

    public void RegisterEnemy(Transform enemyTransform)
    {
        if (!registeredEnemies.Contains(enemyTransform))
        {
            registeredEnemies.Add(enemyTransform);
            Debug.Log($"Enemy registered: {enemyTransform.name}");
        }
    }

    public void UnregisterEnemy(Transform enemyTransform)
    {
        registeredEnemies.Remove(enemyTransform);
        Debug.Log($"Enemy unregistered: {enemyTransform.name}");
    }

    private void UpdateRadar()
    {
        if (player == null || radarRect == null) return;

        // Clean up null/destroyed enemies and their corresponding icons
        for (int i = registeredEnemies.Count - 1; i >= 0; i--)
        {
            if (registeredEnemies[i] == null)
            {
                if (i < enemyIcons.Count && enemyIcons[i] != null)
                {
                    Destroy(enemyIcons[i]);
                    enemyIcons.RemoveAt(i);
                }
                registeredEnemies.RemoveAt(i);
            }
        }

        // Ensure we have icons for all enemies
        while (enemyIcons.Count < registeredEnemies.Count)
        {
            GameObject icon = Instantiate(enemyIconPrefab, radarRect);
            enemyIcons.Add(icon);
        }

        // Update icon positions
        for (int i = 0; i < registeredEnemies.Count; i++)
        {
            var enemy = registeredEnemies[i];
            if (enemy == null || i >= enemyIcons.Count) continue;

            Vector3 offset = enemy.position - player.position;
            float distance = offset.magnitude;

            // Hide icon if out of range, show if in range
            if (distance > radarRange)
            {
                enemyIcons[i].SetActive(false);
            }
            else
            {
                enemyIcons[i].SetActive(true);

                // Convert world position to radar position
                float radarX = (offset.x / radarRange) * (radarRect.rect.width / 2f);
                float radarY = (offset.z / radarRange) * (radarRect.rect.height / 2f);

                // Update position
                RectTransform iconRect = enemyIcons[i].GetComponent<RectTransform>();
                iconRect.anchoredPosition = new Vector2(radarX, radarY);
            }
        }
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }
}