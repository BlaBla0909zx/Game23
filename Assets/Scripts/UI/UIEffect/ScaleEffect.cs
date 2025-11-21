using UnityEngine;
using DG.Tweening;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

[RequireComponent(typeof(Button))]
public class ScaleEffect : MonoBehaviour, IUIEffect, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Scale Settings")]
    [SerializeField] private Vector3 targetScale = new Vector3(1.2f, 1.2f, 1f);
    [SerializeField] private float duration = 0.2f;
    [SerializeField] private bool loop = true;

    [Header("Text Settings")]
    [SerializeField] private bool colorTextChange;
    [SerializeField] private Color targetColor = Color.yellow;
    [SerializeField] private TextMeshProUGUI textComponent;

    private Vector3 originalScale;
    private Color originalColor;
    private Tween scaleTween;
    private Tween colorTween;

    private void Awake()
    {
        originalScale = transform.localScale;

        if (colorTextChange)
        {
            if (textComponent == null)
                textComponent = GetComponentInChildren<TextMeshProUGUI>();

            if (textComponent != null)
                originalColor = textComponent.color;
        }
    }

    public void Play()
    {
        // Kill các tween trước
        scaleTween?.Kill();
        colorTween?.Kill();

        // Scale Tween
        if (loop)
        {
            scaleTween = transform
                .DOScale(targetScale, duration)
                .SetLoops(-1, LoopType.Yoyo)
                .SetEase(Ease.InOutSine);
        }
        else
        {
            scaleTween = transform
                .DOScale(targetScale, duration)
                .SetEase(Ease.InOutSine);
        }

        // Text Color Tween
        if (colorTextChange && textComponent != null)
        {
            colorTween = textComponent
                .DOColor(targetColor, duration)
                .SetEase(Ease.InOutSine);
        }
    }

    public void Stop()
    {
        // Kill tween
        scaleTween?.Kill();
        colorTween?.Kill();

        // Reset về trạng thái ban đầu
        transform.localScale = originalScale;

        if (colorTextChange && textComponent != null)
            textComponent.color = originalColor;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        Play();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        Stop();
    }
}
