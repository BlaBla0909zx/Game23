using System;
using UnityEngine;
using UnityEngine.UI;

public class UIUpgradeSlot : MonoBehaviour
{
    [SerializeField] private TMPro.TextMeshProUGUI statTypeText;
    [SerializeField] private TMPro.TextMeshProUGUI valueText;
    [SerializeField] private TMPro.TextMeshProUGUI cost;
    [SerializeField] private Button upgradeButton;
    public event Action<UIUpgradeSlot> OnUpgradeClicked;

    public StatRarityConfig _statRarityConfig;
    public StatType type;

    public void Setup(StatRarityConfig statRarityConfig, StatType statType)
    {
        _statRarityConfig = statRarityConfig;
        cost.text = _statRarityConfig.cost.ToString();
        GameDataManager.Instance.GetRarityColor(_statRarityConfig.rarity, out Color color);
        statTypeText.color = color;
        statTypeText.text = statType.ToString();
        type = statType;
        valueText.text = TextHelper.FormatStat(statType, _statRarityConfig.value);
        upgradeButton.onClick.RemoveAllListeners();
        upgradeButton.onClick.AddListener(OnClick);

    }
    private void OnClick()
    {
        OnUpgradeClicked?.Invoke(this);
        upgradeButton.interactable = false;
        OnUpgradeClicked = null;
    }

    private void OnDestroy()
    {
        upgradeButton.onClick.RemoveAllListeners();
        OnUpgradeClicked = null;
    }


}