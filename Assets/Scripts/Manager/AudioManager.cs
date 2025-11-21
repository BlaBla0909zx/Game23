using System.Collections.Generic;
using UnityEngine;

public enum AudioType
{
    Music,
    SFX,
}

[System.Serializable]
public class AudioData
{
    public string audioName;
    public AudioClip audioClip;
    public AudioType audioType;
    public bool loop = false;
    public float volume = 1f;
    public float pitch = 1f;
}

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Audio Sources")]
    [SerializeField]
    private AudioSource musicSource;

    [SerializeField]
    private AudioSource sfxSource;

    [Header("Audio Data")]
    [SerializeField]
    private List<AudioData> audioDatabase = new List<AudioData>();

    private Dictionary<string, AudioData> audioDictionary = new Dictionary<string, AudioData>();

    // Current audio settings from SettingsManager
    private AudioSettings currentAudioSettings;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        InitializeAudioSources();
        BuildAudioDictionary();
    }

    private void Start()
    {
        // Subscribe to settings changes
        if (SettingManager.Instance != null)
        {
            SettingManager.Instance.OnSettingsChanged += OnSettingsChanged;
            currentAudioSettings = SettingManager.Instance.playerSetting.audioSettings;
            ApplyAudioSettings();
        }

        // Debug: Check if "bg" exists in database
        if (HasAudio("bg"))
        {
            PlayMusic("bg");
        }
        else
        {
            Debug.LogWarning("AudioManager: 'bg' not found in database. Available audio clips:");
            foreach (var kvp in audioDictionary)
            {
                Debug.Log($"  - '{kvp.Key}' (Type: {kvp.Value.audioType})");
            }
        }
    }

    private void OnDestroy()
    {
        // Unsubscribe from settings changes
        if (SettingManager.Instance != null)
        {
            SettingManager.Instance.OnSettingsChanged -= OnSettingsChanged;
        }
    }

    private void InitializeAudioSources()
    {
        // Create audio sources if they don't exist
        if (musicSource == null)
        {
            GameObject musicObj = new GameObject("MusicSource");
            musicObj.transform.SetParent(transform);
            musicSource = musicObj.AddComponent<AudioSource>();
            musicSource.loop = true;
            musicSource.playOnAwake = false;
        }

        if (sfxSource == null)
        {
            GameObject sfxObj = new GameObject("SFXSource");
            sfxObj.transform.SetParent(transform);
            sfxSource = sfxObj.AddComponent<AudioSource>();
            sfxSource.loop = false;
            sfxSource.playOnAwake = false;
        }
    }

    private void BuildAudioDictionary()
    {
        audioDictionary.Clear();
        foreach (var audioData in audioDatabase)
        {
            if (audioData.audioClip != null && !string.IsNullOrEmpty(audioData.audioName))
            {
                audioDictionary[audioData.audioName] = audioData;
            }
        }
    }

    private void OnSettingsChanged()
    {
        if (SettingManager.Instance != null)
        {
            currentAudioSettings = SettingManager.Instance.playerSetting.audioSettings;
            ApplyAudioSettings();
        }
    }

    private void ApplyAudioSettings()
    {
        if (currentAudioSettings == null)
            return;

        // Apply master volume to all sources
        if (musicSource != null)
            musicSource.volume =
                currentAudioSettings.musicVolume * currentAudioSettings.masterVolume;

        if (sfxSource != null)
            sfxSource.volume = currentAudioSettings.sfxVolume * currentAudioSettings.masterVolume;
    }

    #region Public Audio Playing Methods

    /// <summary>
    /// Play music by name
    /// </summary>
    /// <param name="audioName">Name of the audio clip</param>
    /// <param name="fadeIn">Whether to fade in the music</param>
    /// <param name="fadeTime">Fade time in seconds</param>
    public void PlayMusic(string audioName, bool fadeIn = false, float fadeTime = 1f)
    {
        if (
            audioDictionary.TryGetValue(audioName, out AudioData audioData)
            && audioData.audioType == AudioType.Music
        )
        {
            if (fadeIn)
            {
                StartCoroutine(FadeInMusic(audioData, fadeTime));
            }
            else
            {
                musicSource.clip = audioData.audioClip;
                musicSource.volume =
                    audioData.volume
                    * currentAudioSettings.musicVolume
                    * currentAudioSettings.masterVolume;
                musicSource.pitch = audioData.pitch;
                musicSource.loop = audioData.loop;
                musicSource.Play();
            }
        }
        else
        {
            Debug.LogWarning($"AudioManager: Music '{audioName}' not found in database!");
        }
    }

    /// <summary>
    /// Play SFX by name
    /// </summary>
    /// <param name="audioName">Name of the audio clip</param>
    /// <param name="volumeMultiplier">Volume multiplier (0-1)</param>
    /// <param name="pitchMultiplier">Pitch multiplier</param>
    public void PlaySFX(string audioName, float volumeMultiplier = 1f, float pitchMultiplier = 1f)
    {
        if (
            audioDictionary.TryGetValue(audioName, out AudioData audioData)
            && audioData.audioType == AudioType.SFX
        )
        {
            sfxSource.volume =
                audioData.volume
                * volumeMultiplier
                * currentAudioSettings.sfxVolume
                * currentAudioSettings.masterVolume;
            sfxSource.pitch = audioData.pitch * pitchMultiplier;
            sfxSource.PlayOneShot(audioData.audioClip);
        }
        else
        {
            Debug.LogWarning($"AudioManager: SFX '{audioName}' not found in database!");
        }
    }

    /// <summary>
    /// Play any audio by name (auto-detects type)
    /// </summary>
    /// <param name="audioName">Name of the audio clip</param>
    /// <param name="volumeMultiplier">Volume multiplier (0-1)</param>
    /// <param name="pitchMultiplier">Pitch multiplier</param>
    public void PlayAudio(string audioName, float volumeMultiplier = 1f, float pitchMultiplier = 1f)
    {
        if (audioDictionary.TryGetValue(audioName, out AudioData audioData))
        {
            switch (audioData.audioType)
            {
                case AudioType.Music:
                    PlayMusic(audioName);
                    break;
                case AudioType.SFX:
                    PlaySFX(audioName, volumeMultiplier, pitchMultiplier);
                    break;
            }
        }
        else
        {
            Debug.LogWarning($"AudioManager: Audio '{audioName}' not found in database!");
        }
    }

    #endregion

    #region Audio Control Methods

    /// <summary>
    /// Stop music with optional fade out
    /// </summary>
    /// <param name="fadeOut">Whether to fade out the music</param>
    /// <param name="fadeTime">Fade time in seconds</param>
    public void StopMusic(bool fadeOut = false, float fadeTime = 1f)
    {
        if (fadeOut)
        {
            StartCoroutine(FadeOutMusic(fadeTime));
        }
        else
        {
            musicSource.Stop();
        }
    }

    /// <summary>
    /// Pause music
    /// </summary>
    public void PauseMusic()
    {
        musicSource.Pause();
    }

    /// <summary>
    /// Resume music
    /// </summary>
    public void ResumeMusic()
    {
        musicSource.UnPause();
    }

    /// <summary>
    /// Stop all audio
    /// </summary>
    public void StopAllAudio()
    {
        musicSource.Stop();
        sfxSource.Stop();
    }

    /// <summary>
    /// Set master volume (affects all audio types)
    /// </summary>
    /// <param name="volume">Volume level (0-1)</param>
    public void SetMasterVolume(float volume)
    {
        if (SettingManager.Instance != null)
        {
            SettingManager.Instance.playerSetting.audioSettings.masterVolume = Mathf.Clamp01(
                volume
            );
            ApplyAudioSettings();
        }
    }

    /// <summary>
    /// Set music volume
    /// </summary>
    /// <param name="volume">Volume level (0-1)</param>
    public void SetMusicVolume(float volume)
    {
        if (SettingManager.Instance != null)
        {
            SettingManager.Instance.playerSetting.audioSettings.musicVolume = Mathf.Clamp01(volume);
            ApplyAudioSettings();
        }
    }

    /// <summary>
    /// Set SFX volume
    /// </summary>
    /// <param name="volume">Volume level (0-1)</param>
    public void SetSFXVolume(float volume)
    {
        if (SettingManager.Instance != null)
        {
            SettingManager.Instance.playerSetting.audioSettings.sfxVolume = Mathf.Clamp01(volume);
            ApplyAudioSettings();
        }
    }

    #endregion

    #region Utility Methods

    /// <summary>
    /// Check if audio exists in database
    /// </summary>
    /// <param name="audioName">Name of the audio clip</param>
    /// <returns>True if audio exists</returns>
    public bool HasAudio(string audioName)
    {
        return audioDictionary.ContainsKey(audioName);
    }

    /// <summary>
    /// Get audio data by name
    /// </summary>
    /// <param name="audioName">Name of the audio clip</param>
    /// <returns>AudioData or null if not found</returns>
    public AudioData GetAudioData(string audioName)
    {
        audioDictionary.TryGetValue(audioName, out AudioData audioData);
        return audioData;
    }

    /// <summary>
    /// Add audio to database at runtime
    /// </summary>
    /// <param name="audioData">Audio data to add</param>
    public void AddAudio(AudioData audioData)
    {
        if (audioData.audioClip != null && !string.IsNullOrEmpty(audioData.audioName))
        {
            audioDictionary[audioData.audioName] = audioData;
            audioDatabase.Add(audioData);
        }
    }

    /// <summary>
    /// Remove audio from database
    /// </summary>
    /// <param name="audioName">Name of the audio clip to remove</param>
    public void RemoveAudio(string audioName)
    {
        if (audioDictionary.ContainsKey(audioName))
        {
            audioDictionary.Remove(audioName);
            audioDatabase.RemoveAll(a => a.audioName == audioName);
        }
    }

    #endregion

    #region Coroutines

    private System.Collections.IEnumerator FadeInMusic(AudioData audioData, float fadeTime)
    {
        musicSource.clip = audioData.audioClip;
        musicSource.pitch = audioData.pitch;
        musicSource.loop = audioData.loop;
        musicSource.volume = 0f;
        musicSource.Play();

        float targetVolume =
            audioData.volume * currentAudioSettings.musicVolume * currentAudioSettings.masterVolume;
        float elapsedTime = 0f;

        while (elapsedTime < fadeTime)
        {
            elapsedTime += Time.deltaTime;
            musicSource.volume = Mathf.Lerp(0f, targetVolume, elapsedTime / fadeTime);
            yield return null;
        }

        musicSource.volume = targetVolume;
    }

    private System.Collections.IEnumerator FadeOutMusic(float fadeTime)
    {
        float startVolume = musicSource.volume;
        float elapsedTime = 0f;

        while (elapsedTime < fadeTime)
        {
            elapsedTime += Time.deltaTime;
            musicSource.volume = Mathf.Lerp(startVolume, 0f, elapsedTime / fadeTime);
            yield return null;
        }

        musicSource.Stop();
        musicSource.volume = startVolume; // Restore original volume
    }

    #endregion

    #region Editor Methods

#if UNITY_EDITOR
    [ContextMenu("Build Audio Dictionary")]
    private void BuildAudioDictionaryEditor()
    {
        BuildAudioDictionary();
        Debug.Log($"AudioManager: Built dictionary with {audioDictionary.Count} audio clips");
    }

    [ContextMenu("Debug Audio Database")]
    private void DebugAudioDatabase()
    {
        Debug.Log($"AudioManager: Audio Database contains {audioDatabase.Count} entries:");
        for (int i = 0; i < audioDatabase.Count; i++)
        {
            var audio = audioDatabase[i];
            Debug.Log(
                $"  [{i}] Name: '{audio.audioName}', Type: {audio.audioType}, Clip: {(audio.audioClip != null ? audio.audioClip.name : "NULL")}"
            );
        }

        Debug.Log($"AudioManager: Audio Dictionary contains {audioDictionary.Count} entries:");
        foreach (var kvp in audioDictionary)
        {
            Debug.Log(
                $"  '{kvp.Key}' -> Type: {kvp.Value.audioType}, Clip: {(kvp.Value.audioClip != null ? kvp.Value.audioClip.name : "NULL")}"
            );
        }
    }
#endif

    #endregion
}
