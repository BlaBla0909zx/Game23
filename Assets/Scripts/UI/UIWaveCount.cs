using System;
using System.Collections;
using TMPro;
using UnityEngine;

public class UIWaveCount : MonoBehaviour
{
    public static UIWaveCount Instance { get; private set; }

    public static event Action OnWaveBannerFinished;

    [SerializeField] private TextMeshProUGUI waveText;
    [SerializeField] private RectTransform waveRect;
    [SerializeField] private float moveDuration = 0.6f;
    [SerializeField] private float stayDuration = 5f;
    private Vector2 offScreenLeft;
    private Vector2 offScreenRight;
    private Vector2 centerScreen;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        waveRect.anchorMin = new Vector2(0.5f, 0.5f);
        waveRect.anchorMax = new Vector2(0.5f, 0.5f);
        waveRect.pivot = new Vector2(0.5f, 0.5f);
        centerScreen = Vector2.zero;
        RectTransform canvasRect = waveRect.GetComponentInParent<Canvas>().GetComponent<RectTransform>();
        float width = canvasRect.rect.width;
        offScreenLeft = new Vector2(-width * 0.6f, 0);
        offScreenRight = new Vector2(width * 0.6f, 0);
    }

    private IEnumerator Start()
    {
        yield return new WaitUntil(() => WaveManager.Instance != null);
        // Hiển thị wave hiện tại ngay
        UpdateWaveTextSafe(WaveManager.Instance.GetCurrentWaveNumber() - 1);
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
        if (waveText == null) return;
        int displayWave = waveIndex + 1;
        int totalWaves = WaveManager.Instance.GetTotalWaves();
        waveText.enabled = true; // ensure text is visible for each wave
        waveText.text = $"Wave: {displayWave} / {totalWaves}";
        StopAllCoroutines();
        StartCoroutine(PlayWaveAnimation());
    }

    public float GetAnimationDuration()
    {
        return (moveDuration * 2f) + stayDuration;
    }

    private IEnumerator PlayWaveAnimation()
    {
        waveRect.anchoredPosition = offScreenLeft;
        yield return MoveUI(waveRect, offScreenLeft, centerScreen, moveDuration);
        yield return new WaitForSeconds(stayDuration);
        yield return MoveUI(waveRect, centerScreen, offScreenRight, moveDuration);
        waveText.enabled = false;
        OnWaveBannerFinished?.Invoke();
    }

    private IEnumerator MoveUI(RectTransform rect, Vector2 from, Vector2 to, float duration)
    {
        float t = 0;
        rect.gameObject.SetActive(true);
        while (t < duration)
        {
            t += Time.deltaTime;
            rect.anchoredPosition = Vector2.Lerp(from, to, t / duration);
            yield return null;
        }
        rect.anchoredPosition = to;
    }
}