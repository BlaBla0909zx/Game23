using UnityEngine;
using Global;
using UnityEngine.UI;

public class UILoadingScene : MonoBehaviour
{
    [Header("Main Buttons")]
    [SerializeField] private Button _playButton;
    [SerializeField] private Button _settingsButton;
    [SerializeField] private Transform _settingsPanel;
    [SerializeField] private Button _closeSettingsButton;

    [Header("Option Tabs (Hover Buttons)")]
    [SerializeField] private UIOptionHover[] _optionsHoverEffects; // 0=Audio, 1=Graphics, 2=Controls

    [Header("Settings Panels")]
    [SerializeField] private GameObject _audioPanel;
    [SerializeField] private GameObject _graphicsPanel;
    [SerializeField] private GameObject _controlsPanel;

    [Header("Graphics UI")]
    [SerializeField] private Transform _graphicsContentParent;
    [SerializeField] private UIOptionChoice _resolutionOptionChoice;
    [SerializeField] private UIOptionChoice _graphicsQualityOptionChoice;

    [Header("Audio UI")]
    [SerializeField] private Transform _soundContentParent;
    [SerializeField] private Slider _masterVolumeSlider;
    [SerializeField] private Slider _sfxVolumeSlider;
    [SerializeField] private Slider _musicVolumeSlider;

    [Header(" Control UI")]

    [SerializeField] private UIKeyBind _keyBindPrefab;
    [SerializeField] private Transform _keyBindContentParent;
    [SerializeField] private Button _applySettingsButton;

    private void Start()
    {
        _playButton.onClick.AddListener(OnPlayButtonClicked);
        _settingsButton.onClick.AddListener(OnSettingsButtonClicked);
        _closeSettingsButton.onClick.AddListener(OnCloseSettingsButtonClicked);
        _applySettingsButton.onClick.AddListener(OnApplySettingsButtonClicked);
        _settingsPanel.gameObject.SetActive(false);
        _optionsHoverEffects[0].OnOptionSelected += (o) => ShowPanel(_audioPanel);
        _optionsHoverEffects[1].OnOptionSelected += (o) => ShowPanel(_graphicsPanel);
        _optionsHoverEffects[2].OnOptionSelected += (o) => ShowPanel(_controlsPanel);
        _masterVolumeSlider.onValueChanged.AddListener(OnMasterVolumeChanged);
        _sfxVolumeSlider.onValueChanged.AddListener(OnSfxVolumeChanged);
        _musicVolumeSlider.onValueChanged.AddListener(OnMusicVolumeChanged);
        Global.Utilities.WaitAfter(0.4f, () =>
        {
            InitUI();
        });
        ShowPanel(_audioPanel);
    }
    private void OnMasterVolumeChanged(float value)
    {
        SettingManager.Instance.playerSetting.audioSettings.masterVolume = value;
    }

    private void OnSfxVolumeChanged(float value)
    {
        SettingManager.Instance.playerSetting.audioSettings.sfxVolume = value;
    }

    private void OnMusicVolumeChanged(float value)
    {
        SettingManager.Instance.playerSetting.audioSettings.musicVolume = value;
    }
    private void OnApplySettingsButtonClicked()
    {
        // Apply graphics settings from UI
        ApplyGraphicsSettingsFromUI();

        // Apply all settings
        SettingManager.Instance.ApplyAllSettings();
        OnCloseSettingsButtonClicked();
    }
    private void ApplyGraphicsSettingsFromUI()
    {
        // Apply graphics quality
        int qualityIndex = _graphicsQualityOptionChoice.GetCurrentValue();
        QualitySettings.SetQualityLevel(qualityIndex);

        // Apply resolution (you may need to create resolution options)
        int resolutionIndex = _resolutionOptionChoice.GetCurrentValue();
        ApplyResolutionFromIndex(resolutionIndex);
    }
    private void ApplyResolutionFromIndex(int index)
    {
        // Define common resolutions
        Resolution[] resolutions = {
            new Resolution { width = 1280, height = 720 },
            new Resolution { width = 1920, height = 1080 },
            new Resolution { width = 2560, height = 1440 },
            new Resolution { width = 3840, height = 2160 }
        };

        if (index >= 0 && index < resolutions.Length)
        {
            var res = resolutions[index];
            SettingManager.Instance.playerSetting.graphicsSettings.resolutionWidth = res.width;
            SettingManager.Instance.playerSetting.graphicsSettings.resolutionHeight = res.height;
        }
    }
    private void OnPlayButtonClicked()
    {
        Loader.Load(Loader.Scene.Gameplay);
    }

    private void OnSettingsButtonClicked()
    {
        _settingsPanel.gameObject.SetActive(true);
        ShowPanel(_audioPanel);
    }

    private void OnCloseSettingsButtonClicked()
    {
        _settingsPanel.gameObject.SetActive(false);
    }

    private void OnDestroy()
    {
        _playButton.onClick.RemoveAllListeners();
        _settingsButton.onClick.RemoveAllListeners();
        _closeSettingsButton.onClick.RemoveAllListeners();
    }

    private void InitUI()
    {
        _graphicsQualityOptionChoice.Initialize(System.Enum.GetNames(typeof(GraphicsQuality)));
        _resolutionOptionChoice.Initialize(System.Enum.GetNames(typeof(GraphicsQualityLevel)));
        for (int i = 0; i < SettingManager.Instance.playerSetting.controlSettings.keyBindings.Length; i++)
        {
            UIKeyBind keyBindUI = Instantiate(_keyBindPrefab, _keyBindContentParent);
            keyBindUI.Initialize(SettingManager.Instance.playerSetting.controlSettings.keyBindings[i]);
        }



    }

    /// <summary>
    /// Hiển thị panel được chọn và ẩn những panel khác
    /// </summary>
    private void ShowPanel(GameObject panelToShow)
    {
        _audioPanel.SetActive(panelToShow == _audioPanel);
        _graphicsPanel.SetActive(panelToShow == _graphicsPanel);
        _controlsPanel.SetActive(panelToShow == _controlsPanel);
    }
}



// ------------------------
// ENUMS
// ------------------------
public enum GraphicsQuality { Low, Medium, High, Ultra }
public enum ScreenMode { Fullscreen, Windowed }

public enum GraphicsQualityLevel
{
    Low = 0,
    Medium = 1,
    High = 2,
    Ultra = 3
}


