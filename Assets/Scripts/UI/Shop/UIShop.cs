using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Linq;
using System.Collections.Generic;
using DG.Tweening;

public class UIShop : MonoBehaviour
{
    private Canvas _canvas;
    private CanvasGroup _canvasGroup;
    [SerializeField] private TextMeshProUGUI coinText;
    [SerializeField] private TextMeshProUGUI maxHealthText;
    [SerializeField] private TextMeshProUGUI hpRegen;
    [SerializeField] private TextMeshProUGUI damageText;
    [SerializeField] private TextMeshProUGUI armor;
    [SerializeField] private TextMeshProUGUI lifeSteal;
    [SerializeField] private TextMeshProUGUI lifeStealRate;
    [SerializeField] private TextMeshProUGUI critChance;
    [SerializeField] private TextMeshProUGUI critMultiplier;
    [SerializeField] private Button countinueButton;

    [SerializeField] private StatUpgradeConfigSO statUpgradeConfig;
    [SerializeField] private UIUpgradeSlot UpgradeSlotPrefab;
    [SerializeField] private Transform upgradeSlotParent;
    [SerializeField] private Button RerollButton;


    //skill area
    [SerializeField] private Transform skillSlotParent;
    [SerializeField] private UISkillUpgradeSlot skillSlotPrefab;


    //cach slots 
    private List<UIUpgradeSlot> currentSlots = new();
    private System.Random rng = new System.Random();
    private void Awake()
    {
        _canvas = GetComponent<Canvas>();
        _canvasGroup = GetComponent<CanvasGroup>();
    }

    private void Start()
    {
        PlayerStats.Instance.Coin.Subscribe((o, n) => coinText.text = n.ToString());
        PlayerStats.Instance.OnStatChanged += HandleStatChanged;
        countinueButton.onClick.AddListener(() =>
        {
            GameplayManager.Instance.ResumeGame();  // Å© StartGame() Ç≈ÇÕÇ»Ç≠ ResumeGame()
        });
        RerollButton.onClick.AddListener(Reroll);
        // InitializeSkills();
        Global.Utilities.WaitAfter(0.1f, InitializeSkills);
        Reroll();
    }

    private void HandleStatChanged()
    {
        var stats = PlayerStats.Instance;
        // CharacterStats baseStats = stats.Stats;
        CharacterStats modified = stats.ModifiedStats;
        CharacterStats current = stats.CurrentStats;
        maxHealthText.text = TextHelper.FormatStat(StatType.Health, current.MaxHealth, modified.MaxHealth);
        hpRegen.text = TextHelper.FormatStat(StatType.Health, current.HpRegen, modified.HpRegen);
        damageText.text = TextHelper.FormatStat(StatType.Damage, current.Damage, modified.Damage);
        armor.text = TextHelper.FormatStat(StatType.Armor, current.Armor, modified.Armor);
        lifeSteal.text = TextHelper.FormatStat(StatType.LifeSteal, current.LifeSteal, modified.LifeSteal);
        lifeStealRate.text = TextHelper.FormatStat(StatType.LifeStealRate, current.LifeStealRate, modified.LifeStealRate);
        critChance.text = TextHelper.FormatStat(StatType.CritChance, current.CritChance, modified.CritChance);
        critMultiplier.text = TextHelper.FormatStat(StatType.CritDamage, current.CritMultiplier, modified.CritMultiplier);
    }


    private StatType PickRandomStatType()
    {
        var dict = System.Enum.GetValues(typeof(StatType))
            .Cast<StatType>()
            .ToDictionary(s => s, s => 1f); // equal weight

        var picker = new WeightedRandomPicker<StatType>(dict);
        return picker.GetRandomItem(rng);
    }
    private void Reroll()
    {
        //check coin
        if (PlayerStats.Instance.Coin.Value < 2)
        {
            return;
        }
        else
        {
            PlayerStats.Instance.Coin.Value -= 2;
            currentSlots.Clear();
            foreach (Transform child in upgradeSlotParent)
            {
                Destroy(child.gameObject);
            }

            // pick random 3 option
            for (int i = 0; i < 3; i++)
            {
                var statType = PickRandomStatType();
                var rarity = PickRandomRarity();
                var config = statUpgradeConfig.GetConfig(statType, rarity);

                if (config != null)
                {
                    UIUpgradeSlot slot = Instantiate(UpgradeSlotPrefab, upgradeSlotParent);
                    slot.Setup(config, statType);
                    slot.OnUpgradeClicked += OnUpgradeClicked;
                    currentSlots.Add(slot);
                }
            }
        }
    }


    private void OnUpgradeClicked(UIUpgradeSlot s)
    {
        if (PlayerStats.Instance.Coin.Value < s._statRarityConfig.cost)
        {
            return;
        }
        else
        {
            PlayerStats.Instance.ChangeStat(s.type, s._statRarityConfig.value);
            PlayerStats.Instance.Coin.Value -= s._statRarityConfig.cost;
            currentSlots.Remove(s);
            Destroy(s.gameObject);
        }
    }

    private Rarity PickRandomRarity()
    {
        var dict = new Dictionary<Rarity, float>
        {
            { Rarity.Common, 50f },
            { Rarity.Rare, 30f },
            { Rarity.Epic, 15f },
            { Rarity.Legendary, 5f },
        };

        var picker = new WeightedRandomPicker<Rarity>(dict);
        return picker.GetRandomItem(rng);
    }

    public void ActiveCanvas(bool active)
    {
        _canvas.enabled = active;
        if (active)
        {
            _canvasGroup.DOFade(1, 0.5f);
            Reroll();
        }
        else
        {
            _canvasGroup.DOFade(0, 0.5f);
        }


    }


    private void InitializeSkills()
    {
        for (int i = 0; i < PlayerStats.Instance.SkillInstances.Count; i++)
        {
            SkillID skillID = PlayerStats.Instance.AllSkills[i];
            Skill_Base skillInstance = PlayerStats.Instance.SkillInstances[i];
            AbilitiesSO skillData = GameDataManager.Instance.AllSkills.Find(s => s.skillID == skillID);
            if (skillData != null)
            {
                UISkillUpgradeSlot skillSlot = Instantiate(skillSlotPrefab, skillSlotParent);
                skillSlot.Initialize(skillInstance);
            }
            else
            {
                Debug.LogWarning($"Skill data not found for {skillID}");
            }
        }
    }
}
