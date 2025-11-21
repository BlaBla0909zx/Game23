using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UISkillUpgradeSlot : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Button upgradeButton;
    [SerializeField] private Image skillIcon;
    [SerializeField] private TextMeshProUGUI currentLevelText;
    [SerializeField] private TextMeshProUGUI upgradeCostText;

    private Skill_Base skillInstance;
    private const int MAX_SKILL_LEVEL = 5;

    public void Initialize(Skill_Base skill)
    {
        skillInstance = skill;
        SetupUI();
        SubscribeToEvents();
        UpdateUI();
    }

    private void SetupUI()
    {
        skillIcon.sprite = skillInstance.SkillData.skillIcon;
        upgradeButton.onClick.AddListener(OnUpgradeButtonClicked);
        upgradeCostText.text = skillInstance.SkillData.UpgradeCost.ToString() + " Coins";
    }

    private void SubscribeToEvents()
    {
        skillInstance.SkillLevel.Subscribe(OnSkillLevelChanged, true);
        PlayerStats.Instance.Coin.Subscribe(OnCoinChanged, true);
    }

    private void OnSkillLevelChanged(int oldValue, int newValue)
    {
        UpdateCurrentLevelText();
        UpdateUpgradeButton();
    }

    private void OnCoinChanged(int oldValue, int newValue)
    {
        UpdateUpgradeButton();
    }

    private void UpdateCurrentLevelText()
    {
        currentLevelText.text = $"Level {skillInstance.SkillLevel.Value}";
    }

    private void OnUpgradeButtonClicked()
    {
        if (CanUpgradeSkill())
        {
            int upgradeCost = skillInstance.SkillData.UpgradeCost;
            skillInstance.UpgradeSkill();
            PlayerStats.Instance.Coin.Value -= upgradeCost;
        }
    }

    private void UpdateUpgradeButton()
    {
        upgradeButton.interactable = CanUpgradeSkill();
    }

    private bool CanUpgradeSkill()
    {
        bool hasMaxLevel = skillInstance.SkillLevel.Value >= MAX_SKILL_LEVEL;
        bool hasEnoughCoins = PlayerStats.Instance.Coin.Value >= skillInstance.SkillData.UpgradeCost;

        return !hasMaxLevel && hasEnoughCoins;
    }

    private void UpdateUI()
    {
        UpdateCurrentLevelText();
        UpdateUpgradeButton();
    }

    private void OnDestroy()
    {
        if (upgradeButton != null)
        {
            upgradeButton.onClick.RemoveListener(OnUpgradeButtonClicked);
        }
    }
}