using UnityEngine;
using TMPro;
using DG.Tweening;

public class UIDamageText : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI damageText;

    private CanvasGroup canvasGroup;
    private RectTransform rectTransform;
    private Vector3 initialPosition;
    private Sequence animationSequence;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();

        if (canvasGroup == null) canvasGroup = gameObject.AddComponent<CanvasGroup>();
        if (damageText == null) damageText = GetComponentInChildren<TextMeshProUGUI>();
    }

    public void Setup(float damage, TextType damageType, Vector3 worldPosition, in DamageTextConfig config)
    {
        transform.position = worldPosition;

        // Format text
        damageText.text = FormatDamageText(damage, damageType);

        // Random font size
        float randomFontSize = Random.Range(config.minFontSize, config.maxFontSize + 1);
        damageText.fontSize = randomFontSize;

        // Set color
        damageText.color = GetDamageTypeColor(damageType, config);

        // Critical style
        if (damageType == TextType.Critical)
        {
            damageText.fontSize = randomFontSize * 1.5f;
            damageText.fontStyle = FontStyles.Bold;
        }
        else
        {
            damageText.fontStyle = FontStyles.Normal;
        }

        // Reset animation states
        rectTransform.localScale = Vector3.zero;
        canvasGroup.alpha = 1f;
        initialPosition = rectTransform.localPosition;

        // Play animation
        PlayAnimation(config);
        gameObject.SetActive(true);
    }


    private void PlayAnimation(in DamageTextConfig config)
    {
        if (animationSequence != null) animationSequence.Kill();

        animationSequence = DOTween.Sequence();

        animationSequence.Append(rectTransform.DOScale(Vector3.one, config.scaleDuration).SetEase(Ease.OutBack));
        animationSequence.Join(rectTransform.DOLocalMoveY(initialPosition.y + config.moveDistance, config.scaleDuration + config.fadeDuration).SetEase(Ease.OutQuart));
        animationSequence.Append(canvasGroup.DOFade(0f, config.fadeDuration).SetEase(Ease.InQuart));

        animationSequence.OnComplete(() => gameObject.SetActive(false));
    }
    private Color GetDamageTypeColor(TextType type, in DamageTextConfig config)
    {
        return type switch
        {
            TextType.Critical => config.criticalColor,
            TextType.Heal => config.healColor,
            TextType.Magic => config.magicColor,
            TextType.Exp => config.expColor,
            TextType.Miss => Color.gray,
            _ => config.normalColor,
        };
    }


    private void OnDisable()
    {
        if (animationSequence != null) animationSequence.Kill();
        if (damageText != null)
        {
            damageText.fontSize = 24f;
            damageText.fontStyle = FontStyles.Normal;
        }
    }
    // ...existing code...

    private string FormatDamageText(float damage, TextType damageType)
    {
        string formattedDamage = Mathf.RoundToInt(damage).ToString();
        return damageType switch
        {
            TextType.Critical => $"!{formattedDamage}",
            TextType.Heal => $"+{formattedDamage}",
            TextType.Magic => $"✦{formattedDamage}✦",
            TextType.Miss => $"miss",
            _ => formattedDamage,
        };
    }

    // ...existing code...

    private void OnDestroy()
    {
        if (animationSequence != null) animationSequence.Kill();
    }
}
