using UnityEngine;

public class Fox_Sagent : Skill_Base
{
    private AreaEffectConfig[] configs;

    public Fox_Sagent(AbilitiesSO skillData)
        : base(skillData) { }

    protected override void OnUse()
    {
        base.OnUse();
        float baseDamage =
            PlayerStats.Instance.CurrentStats.Damage * SkillData.skillEffectMultiplier;
        configs = new AreaEffectConfig[]
        {
            new AreaEffectConfig
            {
                effectType = AreaEffectConfig.EffectType.Continuous,
                duration = 1.5f,
                interval = 0.2f,
                activationDelay = 2f,
                damage =
                    baseDamage
                    / 2
                    * (this.SkillLevel.Value)
                    * (1 + (this.SkillLevel.Value - 1) * 0.1f),
            },
            new AreaEffectConfig
            {
                effectType = AreaEffectConfig.EffectType.OneShot,
                activationDelay = 4f,
                damage =
                    baseDamage * this.SkillLevel.Value * (1 + (this.SkillLevel.Value - 1) * 0.1f),
            },
        };
        BaseVisualEffect vfx = VFXPoolManager.Instance.GetEffect(VisualEffectID.Fox_Sagent);
        //get closest enemy
        Transform closest = TargetDetectionControl.instance.GetClosestEnemy();
        if (closest != null)
        {
            vfx.transform.position = closest.position + new Vector3(0, 0.5f, 0);
        }
        vfx.Play(7f);
        DealDamageArea area = vfx.GetComponent<DealDamageArea>();
        if (area != null)
        {
            area.Initialize(configs);
        }
        else
        {
            area = vfx.gameObject.AddComponent<DealDamageArea>();
            area.Initialize(configs);
        }
    }
}
