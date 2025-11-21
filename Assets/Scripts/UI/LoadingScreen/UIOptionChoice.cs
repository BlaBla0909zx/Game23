using TMPro;

using UnityEngine;
using UnityEngine.UI;
public class UIOptionChoice : MonoBehaviour
{
    [SerializeField] private Button leftBtn;
    [SerializeField] private Button rightBtn;
    [SerializeField] private TextMeshProUGUI optionText;

    private string[] options;
    private int _currentIndex;
    private int _currentValue;

    public void Initialize(string[] options, int defaultValue = 0)
    {
        this.options = options;
        _currentIndex = System.Array.IndexOf(options, defaultValue);
        if (_currentIndex < 0)
            _currentIndex = 0;
        _currentValue = _currentIndex;
        UpdateOptionText();

        leftBtn.onClick.RemoveAllListeners();
        rightBtn.onClick.RemoveAllListeners();
        leftBtn.onClick.AddListener(() => ChangeOption(-1));
        rightBtn.onClick.AddListener(() => ChangeOption(1));
    }

    private void OnDestroy()
    {
        leftBtn.onClick.RemoveAllListeners();
        rightBtn.onClick.RemoveAllListeners();
    }

    private void ChangeOption(int direction)
    {
        _currentIndex = (_currentIndex + direction + options.Length) % options.Length;
        _currentValue = _currentIndex;
        UpdateOptionText();
        Debug.Log($"Current value changed to: {_currentValue}");
    }

    private void UpdateOptionText()
    {
        optionText.text = options[_currentIndex];
    }

    public int GetCurrentValue()
    {
        return _currentValue;
    }

    public void SetCurrentValue(int value)
    {
        _currentIndex = value;
        UpdateOptionText();
    }


}