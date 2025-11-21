public static class SkillFactory
{
    public static Skill_Base CreateSkill(AbilitiesSO skillData)
    {
        switch (skillData.skillID)
        {
            case SkillID.Healing_Totem:
                return new Healing_Totem(skillData);
            case SkillID.Shuriken:
                return new Shuriken_Fan(skillData);
            case SkillID.Fox_Sagen:
                return new Fox_Sagent(skillData);
            default:
                return null;
        }
    }
}