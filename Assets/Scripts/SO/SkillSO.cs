using UnityEngine;

[CreateAssetMenu(fileName = "AbilitiesSO", menuName = "GameData/AbilitiesSO")]
public class AbilitiesSO : ScriptableObject
{
    public SkillID skillID;
    public Sprite skillIcon;
    public string skillName;
    public float cooldown;
    public int UpgradeCost;
    public float skillEffectMultiplier = 1; // use to calculate the skill damge, healing
}

public enum SkillID
{
    Healing_Totem = 0,
    Fox_Sagen = 1,
    Shuriken = 2,
}
