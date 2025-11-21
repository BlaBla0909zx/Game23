using UnityEngine;
[System.Serializable]
public class AudioSettings
{
    [Range(0f, 1f)] public float masterVolume = 1f;
    [Range(0f, 1f)] public float musicVolume = 0.8f;
    [Range(0f, 1f)] public float sfxVolume = 0.8f;

    public void ApplySettings()
    {
        // Apply audio settings through AudioManager
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SetMasterVolume(masterVolume);
            AudioManager.Instance.SetMusicVolume(musicVolume);
            AudioManager.Instance.SetSFXVolume(sfxVolume);
        }
    }
}

[System.Serializable]
public class GraphicsSettings
{
    public enum ScreenMode { Fullscreen, Windowed, Borderless }
    public int resolutionWidth = 1920;
    public int resolutionHeight = 1080;
    public int refreshRate = 60;

    public void ApplySettings()
    {
        Screen.SetResolution(resolutionWidth, resolutionHeight, FullScreenMode.FullScreenWindow, refreshRate);
        QualitySettings.vSyncCount = 1; // Enable VSync
        QualitySettings.antiAliasing = 4; // 2x MSAA
        QualitySettings.shadows = ShadowQuality.All; // Enable shadows
    }
}

[System.Serializable]
public class ControlSettings
{
    public KeyBinding[] keyBindings;

    public bool IsKeyBindingUsed(KeyCode key)
    {
        foreach (var binding in keyBindings)
        {
            if (binding.key == key) return true;
        }
        return false;
    }



    public void ApplySettings()
    {

    }
}
[System.Serializable]
public class KeyBinding
{
    public string actionName;
    public KeyCode key;
    public KeyAction action;
}

public enum KeyAction
{
    MoveForward,
    MoveBackward,
    MoveLeft,
    MoveRight,
    Jump,
    Sprint,
    LightAttack,
    HeavyAttack,
    Skill1,
    Skill2,
    Skill3
}

public class SettingManager : MonoBehaviour, ISaveable<PlayerSetting>
{
    public static SettingManager Instance;

    public PlayerSetting playerSetting;

    public event System.Action OnSettingsChanged;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }



    }

    private void Start()
    {
        SaveRegistry.Register<PlayerSetting>(this);
        playerSetting = SaveManager.Instance.LoadData<PlayerSetting>();
        if (playerSetting == null)
        {
            playerSetting = new PlayerSetting();
        }
    }
    public void ApplyAllSettings()
    {
        playerSetting.audioSettings.ApplySettings();
        playerSetting.graphicsSettings.ApplySettings();
        playerSetting.controlSettings.ApplySettings();
        OnSettingsChanged?.Invoke();
    }

    public PlayerSetting GetSaveData()
    {
        return playerSetting;
    }



    public void SetData(PlayerSetting data)
    {
        this.playerSetting = data;
    }

    #region Audio Settings Methods

    /// <summary>
    /// Set master volume and apply settings
    /// </summary>
    /// <param name="volume">Volume level (0-1)</param>
    public void SetMasterVolume(float volume)
    {
        playerSetting.audioSettings.masterVolume = Mathf.Clamp01(volume);
        playerSetting.audioSettings.ApplySettings();
        OnSettingsChanged?.Invoke();
    }

    /// <summary>
    /// Set music volume and apply settings
    /// </summary>
    /// <param name="volume">Volume level (0-1)</param>
    public void SetMusicVolume(float volume)
    {
        playerSetting.audioSettings.musicVolume = Mathf.Clamp01(volume);
        playerSetting.audioSettings.ApplySettings();
        OnSettingsChanged?.Invoke();
    }

    /// <summary>
    /// Set SFX volume and apply settings
    /// </summary>
    /// <param name="volume">Volume level (0-1)</param>
    public void SetSFXVolume(float volume)
    {
        playerSetting.audioSettings.sfxVolume = Mathf.Clamp01(volume);
        playerSetting.audioSettings.ApplySettings();
        OnSettingsChanged?.Invoke();
    }

    /// <summary>
    /// Get current master volume
    /// </summary>
    /// <returns>Master volume (0-1)</returns>
    public float GetMasterVolume()
    {
        return playerSetting.audioSettings.masterVolume;
    }

    /// <summary>
    /// Get current music volume
    /// </summary>
    /// <returns>Music volume (0-1)</returns>
    public float GetMusicVolume()
    {
        return playerSetting.audioSettings.musicVolume;
    }

    /// <summary>
    /// Get current SFX volume
    /// </summary>
    /// <returns>SFX volume (0-1)</returns>
    public float GetSFXVolume()
    {
        return playerSetting.audioSettings.sfxVolume;
    }

    #endregion

    #region Graphics Settings Methods

    /// <summary>
    /// Set screen resolution and apply settings
    /// </summary>
    /// <param name="width">Screen width</param>
    /// <param name="height">Screen height</param>
    /// <param name="refreshRate">Refresh rate (optional)</param>
    public void SetResolution(int width, int height, int refreshRate = 60)
    {
        playerSetting.graphicsSettings.resolutionWidth = width;
        playerSetting.graphicsSettings.resolutionHeight = height;
        playerSetting.graphicsSettings.refreshRate = refreshRate;
        playerSetting.graphicsSettings.ApplySettings();
        OnSettingsChanged?.Invoke();
    }

    /// <summary>
    /// Set graphics quality level
    /// </summary>
    /// <param name="qualityLevel">Quality level (0-5)</param>
    public void SetGraphicsQuality(int qualityLevel)
    {
        QualitySettings.SetQualityLevel(Mathf.Clamp(qualityLevel, 0, QualitySettings.names.Length - 1));
        OnSettingsChanged?.Invoke();
    }

    /// <summary>
    /// Set VSync on/off
    /// </summary>
    /// <param name="enabled">VSync enabled</param>
    public void SetVSync(bool enabled)
    {
        QualitySettings.vSyncCount = enabled ? 1 : 0;
        OnSettingsChanged?.Invoke();
    }

    /// <summary>
    /// Set anti-aliasing level
    /// </summary>
    /// <param name="level">Anti-aliasing level (0, 2, 4, 8)</param>
    public void SetAntiAliasing(int level)
    {
        QualitySettings.antiAliasing = level;
        OnSettingsChanged?.Invoke();
    }

    /// <summary>
    /// Set shadow quality
    /// </summary>
    /// <param name="quality">Shadow quality</param>
    public void SetShadowQuality(ShadowQuality quality)
    {
        QualitySettings.shadows = quality;
        OnSettingsChanged?.Invoke();
    }

    /// <summary>
    /// Get current resolution
    /// </summary>
    /// <returns>Resolution as Vector2Int (width, height)</returns>
    public Vector2Int GetResolution()
    {
        return new Vector2Int(playerSetting.graphicsSettings.resolutionWidth, playerSetting.graphicsSettings.resolutionHeight);
    }

    /// <summary>
    /// Get current refresh rate
    /// </summary>
    /// <returns>Refresh rate</returns>
    public int GetRefreshRate()
    {
        return playerSetting.graphicsSettings.refreshRate;
    }

    #endregion

    #region Control Settings Methods

    /// <summary>
    /// Set key binding for a specific action
    /// </summary>
    /// <param name="action">The action to bind</param>
    /// <param name="newKey">The new key to bind</param>
    /// <returns>True if binding was successful</returns>
    public bool SetKeyBinding(KeyAction action, KeyCode newKey)
    {
        // Check if key is already used
        if (playerSetting.controlSettings.IsKeyBindingUsed(newKey))
        {
            Debug.LogWarning($"Key {newKey} is already bound to another action!");
            return false;
        }

        // Find and update the binding
        for (int i = 0; i < playerSetting.controlSettings.keyBindings.Length; i++)
        {
            if (playerSetting.controlSettings.keyBindings[i].action == action)
            {
                playerSetting.controlSettings.keyBindings[i].key = newKey;
                OnSettingsChanged?.Invoke();
                return true;
            }
        }

        Debug.LogWarning($"Action {action} not found in key bindings!");
        return false;
    }

    /// <summary>
    /// Get key binding for a specific action
    /// </summary>
    /// <param name="action">The action to get binding for</param>
    /// <returns>KeyCode for the action, or KeyCode.None if not found</returns>
    public KeyCode GetKeyBinding(KeyAction action)
    {
        foreach (var binding in playerSetting.controlSettings.keyBindings)
        {
            if (binding.action == action)
            {
                return binding.key;
            }
        }
        return KeyCode.None;
    }

    /// <summary>
    /// Get action name for a specific action
    /// </summary>
    /// <param name="action">The action to get name for</param>
    /// <returns>Action name string</returns>
    public string GetActionName(KeyAction action)
    {
        foreach (var binding in playerSetting.controlSettings.keyBindings)
        {
            if (binding.action == action)
            {
                return binding.actionName;
            }
        }
        return action.ToString();
    }

    /// <summary>
    /// Reset key binding to default
    /// </summary>
    /// <param name="action">The action to reset</param>
    public void ResetKeyBinding(KeyAction action)
    {
        // Reset to default key based on action
        KeyCode defaultKey = GetDefaultKey(action);
        SetKeyBinding(action, defaultKey);
    }

    /// <summary>
    /// Get default key for an action
    /// </summary>
    /// <param name="action">The action</param>
    /// <returns>Default KeyCode for the action</returns>
    private KeyCode GetDefaultKey(KeyAction action)
    {
        switch (action)
        {
            case KeyAction.LightAttack: return KeyCode.Mouse0;
            case KeyAction.HeavyAttack: return KeyCode.Mouse1;
            case KeyAction.Skill1: return KeyCode.J;
            case KeyAction.Skill2: return KeyCode.K;
            case KeyAction.Skill3: return KeyCode.Alpha3;
            case KeyAction.Jump: return KeyCode.Space;
            case KeyAction.Sprint: return KeyCode.LeftShift;
            default: return KeyCode.None;
        }
    }

    #endregion

    #region Utility Methods

    /// <summary>
    /// Reset all settings to default values
    /// </summary>
    public void ResetToDefaults()
    {
        playerSetting = new PlayerSetting();
        ApplyAllSettings();
        Debug.Log("Settings reset to defaults");
    }

    /// <summary>
    /// Save current settings to file
    /// </summary>
    public void SaveSettings()
    {
        SaveManager.Instance.SaveData<PlayerSetting>();
        Debug.Log("Settings saved");
    }

    /// <summary>
    /// Load settings from file
    /// </summary>
    public void LoadSettings()
    {
        playerSetting = SaveManager.Instance.LoadData<PlayerSetting>();
        if (playerSetting == null)
        {
            playerSetting = new PlayerSetting();
        }
        ApplyAllSettings();
        Debug.Log("Settings loaded");
    }

    /// <summary>
    /// Check if settings have been modified
    /// </summary>
    /// <returns>True if settings are different from defaults</returns>
    public bool HasModifiedSettings()
    {
        var defaultSettings = new PlayerSetting();
        return !playerSetting.Equals(defaultSettings);
    }

    /// <summary>
    /// Get all available resolutions
    /// </summary>
    /// <returns>Array of available resolutions</returns>
    public Resolution[] GetAvailableResolutions()
    {
        return Screen.resolutions;
    }

    /// <summary>
    /// Get all available quality levels
    /// </summary>
    /// <returns>Array of quality level names</returns>
    public string[] GetAvailableQualityLevels()
    {
        return QualitySettings.names;
    }

    /// <summary>
    /// Get current quality level
    /// </summary>
    /// <returns>Current quality level index</returns>
    public int GetCurrentQualityLevel()
    {
        return QualitySettings.GetQualityLevel();
    }

    #endregion
}
[System.Serializable]
[SaveData("PlayerSettings.json")]
public class PlayerSetting
{
    public AudioSettings audioSettings;
    public GraphicsSettings graphicsSettings;
    public ControlSettings controlSettings;
    public PlayerSetting()
    {
        audioSettings = new AudioSettings();
        graphicsSettings = new GraphicsSettings();
        controlSettings = new ControlSettings();
        var n = new KeyBinding[]
        {
            // new KeyBinding { actionName = "Jump", key = KeyCode.Space, action = KeyAction.Jump },
            // new KeyBinding { actionName = "Sprint", key = KeyCode.LeftShift, action = KeyAction.Sprint },
            new KeyBinding { actionName = "Light Attack", key = KeyCode.Mouse0, action = KeyAction.LightAttack },
            new KeyBinding { actionName = "Heavy Attack", key = KeyCode.Mouse1, action = KeyAction.HeavyAttack },
            new KeyBinding { actionName = "Skill 1", key = KeyCode.J, action = KeyAction.Skill1 },
            new KeyBinding { actionName = "Skill 2", key = KeyCode.K, action = KeyAction.Skill2 },
            new KeyBinding { actionName = "Skill 3", key = KeyCode.Alpha3, action = KeyAction.Skill3 },

        };
        controlSettings.keyBindings = n;
    }
}