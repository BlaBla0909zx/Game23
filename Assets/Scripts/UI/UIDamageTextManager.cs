using System.Collections.Generic;
using UnityEngine;

public class UIDamageTextManager : MonoBehaviour
{
    public static UIDamageTextManager Instance { get; private set; }

    [Header("Prefab & Pool")]
    [SerializeField] private UIDamageText damageTextPrefab;
    [SerializeField] private int poolSize = 30;
    [SerializeField] private float displayDuration = 2f;

    [Header("Damage Type Colors")]
    public Color normalColor = Color.white;
    public Color criticalColor = Color.red;
    public Color healColor = Color.green;
    public Color magicColor = Color.cyan;
    public Color expColor = Color.yellow;

    [Header("Animation Settings")]
    public float scaleDuration = 0.5f;
    public float fadeDuration = 1f;
    public float moveDistance = 50f;
    public int minFontSize = 36;
    public int maxFontSize = 60;

    private readonly List<DamageTextData> activeDamageTexts = new();
    private readonly Queue<UIDamageText> damageTextPool = new();
    private Canvas _canvas;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        _canvas = GetComponent<Canvas>();
        InitializePool();
        cachedConfig = new DamageTextConfig
        {
            normalColor = normalColor,
            criticalColor = criticalColor,
            healColor = healColor,
            magicColor = magicColor,
            expColor = expColor,
            scaleDuration = scaleDuration,
            fadeDuration = fadeDuration,
            moveDistance = moveDistance,
            minFontSize = minFontSize,
            maxFontSize = maxFontSize
        };
    }
    private DamageTextConfig cachedConfig;
    private void InitializePool()
    {
        for (int i = 0; i < poolSize; i++)
        {
            UIDamageText damageText = Instantiate(damageTextPrefab, transform);
            damageText.gameObject.SetActive(false);
            damageTextPool.Enqueue(damageText);
        }
    }

    private void Update()
    {
        CheckExpiredTexts();
    }

    private void CheckExpiredTexts()
    {
        for (int i = activeDamageTexts.Count - 1; i >= 0; i--)
        {
            var damageTextData = activeDamageTexts[i];
            if (Time.time >= damageTextData.expiryTime)
            {
                ReturnToPool(damageTextData.damageText);
                activeDamageTexts.RemoveAt(i);
            }
        }
    }

   
public void ShowDamageText(Vector3 worldPosition, float damageAmount, TextType damageType = TextType.Normal)
{
    UIDamageText damageText = GetFromPool();

    Vector3 randomOffset = new Vector3(
        Random.Range(-0.5f, 0.5f),
        Random.Range(1f, 2f),
        0f
    );

    Vector3 finalPosition = worldPosition + randomOffset;
    damageText.Setup(damageAmount, damageType, finalPosition, cachedConfig);
    activeDamageTexts.Add(new DamageTextData
    {
        worldPosition = finalPosition,
        damageText = damageText,
        expiryTime = Time.time + displayDuration
    });
}

    private UIDamageText GetFromPool()
    {
        if (damageTextPool.Count > 0)
            return damageTextPool.Dequeue();
        else
            return Instantiate(damageTextPrefab, transform);
    }

    private void ReturnToPool(UIDamageText damageText)
    {
        damageText.gameObject.SetActive(false);
        damageTextPool.Enqueue(damageText);
    }

    public void ClearAllDamageTexts()
    {
        foreach (var damageTextData in activeDamageTexts)
            ReturnToPool(damageTextData.damageText);

        activeDamageTexts.Clear();
    }
}

public struct DamageTextData
{
    public Vector3 worldPosition;
    public UIDamageText damageText;
    public float expiryTime;
}

public enum TextType
{
    Normal,
    Critical,
    Heal,
    Magic,
    Exp,
    Miss
}


public struct DamageTextConfig
{
    public Color normalColor;
    public Color criticalColor;
    public Color healColor;
    public Color magicColor;
    public Color expColor;
    public float scaleDuration;
    public float fadeDuration;
    public float moveDistance;
    public int minFontSize;
    public int maxFontSize;
}