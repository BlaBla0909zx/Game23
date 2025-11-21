using UnityEngine;
public abstract class Skill_Base : ISkill
{
    public AbilitiesSO SkillData { get; private set; }
    public virtual bool IsOnCooldown => CooldownTimer.Value > 0f;
    public ObservableValue<float> CooldownTimer { get; private set; }

    public ObservableValue<int> SkillLevel { get; private set; } = new ObservableValue<int>(1);

    protected Skill_Base(AbilitiesSO skillData)
    {
        SkillData = skillData;
        CooldownTimer = new ObservableValue<float>(0f);
    }

    public virtual bool CanUse()
    {
        return !IsOnCooldown;
    }

    public virtual bool TryUse()
    {
        if (!CanUse())
            return false;

        Use();
        return true;
    }

    public virtual void Use()
    {
        if (!CanUse())
            return;

        StartCooldown();
        OnUse();
    }

    protected virtual void OnUse()
    {
        // Override in derived classes for skill-specific behavior
    }

    protected void StartCooldown()
    {
        CooldownTimer.Value = SkillData.cooldown;
    }

    public virtual void UpdateCooldown(float deltaTime)
    {
        CooldownTimer.Value = Mathf.Max(0f, CooldownTimer.Value - deltaTime);

    }

    //upgrade skill level
    public void UpgradeSkill()
    {
        SkillLevel.Value++;
    }


}

public interface ISkill
{
    AbilitiesSO SkillData { get; }
    bool IsOnCooldown { get; }
    ObservableValue<float> CooldownTimer { get; }

    bool CanUse();
    bool TryUse();
    void Use();
    void UpdateCooldown(float deltaTime);
    void UpgradeSkill();
}