using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class StatRarityConfig
{
    public Rarity rarity;
    public int cost;
    public float value; // giá trị tăng thêm
}

[System.Serializable]
public class StatConfig
{
    public StatType statType;
    public List<StatRarityConfig> rarityConfigs = new List<StatRarityConfig>();
}

[CreateAssetMenu(fileName = "StatUpgradeConfig", menuName = "Configs/StatUpgradeConfig")]
public class StatUpgradeConfigSO : ScriptableObject
{
    public List<StatConfig> statConfigs = new List<StatConfig>();

    public StatRarityConfig GetConfig(StatType type, Rarity rarity)
    {
        var stat = statConfigs.Find(s => s.statType == type);
        if (stat != null)
        {
            return stat.rarityConfigs.Find(r => r.rarity == rarity);
        }
        return null;
    }
}
