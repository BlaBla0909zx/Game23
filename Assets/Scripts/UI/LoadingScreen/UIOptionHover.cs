using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;



public class UIOptionHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [SerializeField] private TextMeshProUGUI optionText;

    public event System.Action<UIOptionHover> OnOptionSelected;
    public event System.Action<UIOptionHover> OnOptionHovered;


    private void Start()
    {
        Color color = optionText.color;
        color.a = 0.8f;
        optionText.color = color;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        Color color = optionText.color;
        color.a = 1f;
        optionText.color = color;
        OnOptionHovered?.Invoke(this);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        Color color = optionText.color;
        color.a = 0.8f;
        optionText.color = color;
    }
    public void OnPointerClick(PointerEventData eventData)
    {
        OnOptionSelected?.Invoke(this);
    }
}

