using UnityEngine;

[System.Serializable]
public struct AreaEffectConfig
{
    [Header("Timing Settings")]
    public float activationDelay;
    public float duration;
    public float interval;
    public float damage;
    public enum EffectType
    {
        Continuous,   // Dùng duration + interval
        OneShot       // Chỉ trigger 1 lần ngay khi active
    }
    public EffectType effectType;

    // Helper properties for validation
    public bool IsOneShot => effectType == EffectType.OneShot;
    public bool IsContinuous => effectType == EffectType.Continuous;
}

public interface IAreaEffect
{
    void Initialize(AreaEffectConfig[] configs);
    void Stop();
    bool IsRunning { get; }
}