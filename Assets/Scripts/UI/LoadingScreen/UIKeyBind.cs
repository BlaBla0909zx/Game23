using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class UIKeyBind : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI actionText;
    [SerializeField] private TextMeshProUGUI keyText;
    [SerializeField] private Button focusBtn;

    private string _actionName;
    private KeyCode _currentKey;
    public bool _isListening = false;

    public void Initialize(KeyBinding binding)
    {
        _actionName = binding.actionName;
        _currentKey = binding.key;
        actionText.text = _actionName;
        keyText.text = _currentKey.ToString();
        focusBtn.onClick.RemoveAllListeners();
        focusBtn.onClick.AddListener(StartListening);
    }

    private void OnDestroy()
    {
        focusBtn.onClick.RemoveListener(StartListening);
    }

    private void StartListening()
    {


        if (_isListening)
        {
            return;
        }

        _isListening = true;
        keyText.text = "<press key>";
        keyText.color = Color.yellow;

    }

    private void StopListening()
    {
        _isListening = false;
        keyText.color = Color.white;
    }

    private void Update()
    {
        if (!_isListening) return;

        foreach (KeyCode key in System.Enum.GetValues(typeof(KeyCode)))
        {
            if (Input.GetKeyDown(key))
            {
                // Check if the new key is already used by OTHER bindings
                bool isKeyUsedByOther = false;
                foreach (var binding in SettingManager.Instance.playerSetting.controlSettings.keyBindings)
                {
                    if (binding.actionName != _actionName && binding.key == key)
                    {
                        isKeyUsedByOther = true;
                        break;
                    }
                }

                if (isKeyUsedByOther)
                {
                    // Keep old key - key is already used by another action
                    keyText.text = _currentKey.ToString();
                    keyText.color = Color.red; // Show error
                }
                else
                {
                    // Update to new key
                    _currentKey = key;
                    keyText.text = key.ToString();

                    // Update key in SettingManager
                    foreach (var binding in SettingManager.Instance.playerSetting.controlSettings.keyBindings)
                    {
                        if (binding.actionName == _actionName)
                        {
                            binding.key = key;
                            break;
                        }
                    }
                }

                StopListening();
                break;
            }
        }

        // Nếu người dùng bấm Escape => hủy lắng nghe
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            keyText.text = _currentKey.ToString();
            StopListening();
        }
    }
}
