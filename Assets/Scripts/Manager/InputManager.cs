using UnityEngine;
using System;
using System.Collections.Generic;
using UnityEngine.InputSystem;
public class InputManager : MonoBehaviour
{
    public static InputManager Instance { get; private set; }

    public PlayerInput input;

    private Dictionary<KeyAction, Action> actionCallbacks = new();

    private Dictionary<KeyAction, KeyCode> keyMappings = new();


    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        // controlSettings = SettingManager.Instance.playerSetting.controlSettings;
        SettingManager.Instance.OnSettingsChanged += ReloadKeyMappings;
        ReloadKeyMappings();
    }
    private void OnDestroy()
    {
        SettingManager.Instance.OnSettingsChanged -= ReloadKeyMappings;
    }
    private void Update()
    {
        foreach (var kvp in keyMappings)
        {
            if (Input.GetKeyDown(kvp.Value))
            {
                if (actionCallbacks.TryGetValue(kvp.Key, out var callback))
                    callback?.Invoke();
            }
        }
    }

    #region Register / Unregister
    public void RegisterKeyAction(KeyAction action, Action callback)
    {
        if (!actionCallbacks.ContainsKey(action))
            actionCallbacks[action] = callback;
        else
            actionCallbacks[action] += callback;
    }

    public void UnregisterKeyAction(KeyAction action, Action callback)
    {
        if (actionCallbacks.ContainsKey(action))
        {
            actionCallbacks[action] -= callback;
            if (actionCallbacks[action] == null)
                actionCallbacks.Remove(action);
        }
    }
    #endregion

    #region Key Mapping Reload
    // Cập nhật lại khi đổi keybinding
    public void ReloadKeyMappings()
    {
        keyMappings.Clear();
        ControlSettings controlSettings = SettingManager.Instance.playerSetting.controlSettings;
        if (controlSettings == null) return;

        foreach (var binding in controlSettings.keyBindings)
        {
            keyMappings[binding.action] = binding.key;
        }
    }
    #endregion

    // Helper: Lấy key hiện tại cho action
    public KeyCode GetKeyForAction(KeyAction action)
    {
        if (keyMappings.TryGetValue(action, out var key))
            return key;
        return KeyCode.None;
    }
}
