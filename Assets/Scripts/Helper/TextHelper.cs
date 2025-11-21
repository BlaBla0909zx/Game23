public static class TextHelper
{
    public static DisplayStatType GetDisplayType(StatType statType)
    {
        switch (statType)
        {
            case StatType.CritChance:
            case StatType.LifeSteal:
            case StatType.LifeStealRate:
            case StatType.Evasion:
            case StatType.CritDamage:
                return DisplayStatType.Percentage;

            case StatType.Health:
            case StatType.Armor:
            case StatType.Damage:

            default:
                return DisplayStatType.Number;
        }
    }

    public static string FormatStat(StatType statType, float baseValue, float bonusValue)
    {
        DisplayStatType displayType = GetDisplayType(statType);

        string formattedBase;
        string formattedBonus;

        if (displayType == DisplayStatType.Percentage)
        {
            formattedBase = $"{baseValue * 100f:0.#}%";
            formattedBonus = bonusValue != 0 ? $" <color=green>(+{bonusValue * 100f:0.#}%)</color>" : "";
        }
        else // Number
        {
            formattedBase = $"{baseValue:0.#}";
            formattedBonus = bonusValue != 0 ? $" <color=green>(+{bonusValue:0.#})</color>" : "";
        }

        return $"{statType}: {formattedBase}{formattedBonus}";
    }

    public static string FormatStat(StatType statType, float value)
    {
        DisplayStatType displayType = GetDisplayType(statType);
        if (displayType == DisplayStatType.Percentage)
        {
            return $"{value * 100f:0.#}%";
        }
        else // Number
        {
            return $"{value:0.#}";
        }
    }
}
