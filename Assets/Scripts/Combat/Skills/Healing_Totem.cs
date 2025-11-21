using UnityEngine;

public class Healing_Totem : Skill_Base
{
    private AreaEffectConfig[] configs;

    public Healing_Totem(AbilitiesSO skillData)
        : base(skillData) { }

    protected override void OnUse()
    {
        base.OnUse();
        float baseRegen = PlayerStats.Instance.CurrentStats.HpRegen * SkillData.skillEffectMultiplier;
        configs = new AreaEffectConfig[]
        {
            new AreaEffectConfig
            {
                effectType = AreaEffectConfig.EffectType.Continuous,
                duration = 8f,
                interval = 1f,
                activationDelay = 0.5f,
                damage = (5 + baseRegen) + (this.SkillLevel.Value - 1) * 2,
            },
        };
        BaseVisualEffect vfx = VFXPoolManager.Instance.GetEffect(VisualEffectID.Healing_Totem);
        vfx.transform.position = PlayerStats.Instance.transform.position;
        vfx.Play();

        HealingArea area = vfx.GetComponent<HealingArea>();
        if (area == null)
        {
            area = vfx.gameObject.AddComponent<HealingArea>();
        }

        area.Initialize(configs);
    }
}
