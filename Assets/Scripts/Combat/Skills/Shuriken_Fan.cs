using UnityEngine;

public class Shuriken_Fan : Skill_Base
{
    private AreaEffectConfig[] configs;

    public Shuriken_Fan(AbilitiesSO skillData)
        : base(skillData)
    {
        float baseDamage =
            PlayerStats.Instance.CurrentStats.Damage * SkillData.skillEffectMultiplier;
        configs = new AreaEffectConfig[]
        {
            new AreaEffectConfig
            {
                effectType = AreaEffectConfig.EffectType.Continuous,
                duration = 1.5f,
                interval = 0.3f,
                activationDelay = 0.08f,
                damage = baseDamage / 2,
            },
        };
    }

    protected override void OnUse()
    {
        base.OnUse();

        BaseVisualEffect vfx = VFXPoolManager.Instance.GetEffect(VisualEffectID.Shuriken_Fan);
        float yRotation = PlayerStats.Instance.transform.eulerAngles.y;
        vfx.transform.position = PlayerStats.Instance.transform.position + new Vector3(0, 0.5f, 0);
        vfx.transform.rotation = Quaternion.Euler(0, yRotation, 0);
        vfx.Play();

        DealDamageArea area = vfx.GetComponent<DealDamageArea>();
        if (area == null)
        {
            area = vfx.gameObject.AddComponent<DealDamageArea>();
        }

        area.Initialize(configs);
    }
}
