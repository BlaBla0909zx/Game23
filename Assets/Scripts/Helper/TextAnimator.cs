using DG.Tweening;
using TMPro;
using UnityEngine.UI;


public static class TextAnimator
{
    public static void AnimateInt(this TextMeshProUGUI text, int newValue, float duration = 0.3f)
    {
        int oldValue = 0;
        int.TryParse(text.text, out oldValue);

        DOTween.To(() => oldValue, x =>
        {
            oldValue = x;
            text.text = oldValue.ToString();
        }, newValue, duration);
    }

    public static void AnimateFloat(TextMeshProUGUI text, float newValue, string format = "0.##", float duration = 0.3f)
    {
        float oldValue = 0f;
        float.TryParse(text.text.Replace("%", "").Replace("x", ""), out oldValue);

        DOTween.To(() => oldValue, x =>
        {
            oldValue = x;
            text.text = oldValue.ToString(format);
        }, newValue, duration);
    }
    
    public static void AnimateFillAmount(Image image, float newValue, float duration = 0.3f)
    {
        float oldValue = image.fillAmount;

        DOTween.To(() => oldValue, x =>
        {
            oldValue = x;
            image.fillAmount = oldValue;
        }, newValue, duration);
    }
}