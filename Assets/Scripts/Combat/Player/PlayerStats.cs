using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStats : MonoBehaviour, IHealthBar, IDamageable
{
    public static PlayerStats Instance { get; private set; }

    [Header("Currency")]
    [SerializeField] private ObservableValue<int> coin = new(0);
    public ObservableValue<int> Coin => coin;

    public CharacterStats Stats = new CharacterStats()
    {
        MaxHealth = 100f,
        CurrentHealth = 100f,
        MoveSpeed = 5f,
        Damage = 10f,
        AttackSpeed = 1f,
        AttackRange = 1f,
        CritChance = 0.1f,
        CritMultiplier = 1.5f,
        Armor = 5f,
        Evasion = 0.05f,
        LifeSteal = 0.05f,
        LifeStealRate = 0.1f,
        HpRegen = 1f
    };

    public CharacterStats ModifiedStats = new CharacterStats(); //stats after item and buffs

    public CharacterStats CurrentStats
    {
        get
        {
            return new CharacterStats()
            {
                MaxHealth = Stats.MaxHealth + ModifiedStats.MaxHealth,
                CurrentHealth = Stats.CurrentHealth, // current health is managed separately
                MoveSpeed = Stats.MoveSpeed + ModifiedStats.MoveSpeed,
                Damage = Stats.Damage + ModifiedStats.Damage,
                AttackSpeed = Stats.AttackSpeed + ModifiedStats.AttackSpeed,
                AttackRange = Stats.AttackRange + ModifiedStats.AttackRange,
                CritChance = Stats.CritChance + ModifiedStats.CritChance,
                CritMultiplier = Stats.CritMultiplier + ModifiedStats.CritMultiplier,
                Armor = Stats.Armor + ModifiedStats.Armor,
                Evasion = Stats.Evasion + ModifiedStats.Evasion,
                LifeSteal = Stats.LifeSteal + ModifiedStats.LifeSteal,
                LifeStealRate = Stats.LifeStealRate + ModifiedStats.LifeStealRate,
                HpRegen = Stats.HpRegen + ModifiedStats.HpRegen
            };
        }
    }

    public float CurrentHP;
    private UIHealthBar _healthBar;
    private float timer;
    
    // Death detection
    private bool isDead = false;
    
    // Events
    public event System.Action OnPlayerDeath;
    public event System.Action OnHealthChanged;
    private float regenInterval = 1f;
    public List<SkillID> AllSkills = new List<SkillID>();
    public List<Skill_Base> SkillInstances = new List<Skill_Base>();

    private KeyCode Skill_1 = KeyCode.J;
    private KeyCode Skill_2 = KeyCode.K;

    public event Action OnStatChanged;

    public event Action OnDataInit;


    private void RegisterAllSkills()
    {
        InputManager.Instance.RegisterKeyAction(KeyAction.Skill1, () => UseSkill(0));
        InputManager.Instance.RegisterKeyAction(KeyAction.Skill2, () => UseSkill(1));
        InputManager.Instance.RegisterKeyAction(KeyAction.Skill3, () => UseSkill(2));

        for (int i = 0; i < AllSkills.Count; i++)
        {
            SkillID skillID = AllSkills[i];
            AbilitiesSO skillData = GameDataManager.Instance.AllSkills.Find(s => s.skillID == skillID);
            if (skillData != null)
            {
                Skill_Base skillInstance = SkillFactory.CreateSkill(skillData);
                if (skillInstance != null)
                {
                    SkillInstances.Add(skillInstance);
                }
                else
                {
                    Debug.LogWarning($"Failed to create skill instance for {skillID}");
                }
            }
            else
            {
                Debug.LogWarning($"Skill data not found for {skillID}");
            }
        }
    }

    private void UpdateSkillCooldowns(float deltaTime)
    {
        foreach (var skill in SkillInstances)
        {
            skill.UpdateCooldown(deltaTime);
        }
    }

    private void UseSkill(int index)
    {
        if (index < 0 || index >= SkillInstances.Count)
        {
            Debug.LogWarning($"Invalid skill index: {index}");
            return;
        }

        Skill_Base skill = SkillInstances[index];
        if (skill.IsOnCooldown)
        {
            return;
        }

        skill.TryUse();
    }
    private void Awake()
    {
        if (Instance == null) Instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }

    }

    private void Start()
    {
        RegisterHealthBar(transform, CurrentStats.MaxHealth, 2f);
        CurrentHP = CurrentStats.CurrentHealth;
        RegisterAllSkills();
        OnDataInit?.Invoke();
    }

    private void Update()
    {
        timer += Time.deltaTime;
        if (timer >= regenInterval)
        {
            timer = 0f;
            HandleRegen(CurrentStats.HpRegen);
        }
        UpdateSkillCooldowns(Time.deltaTime);
    }

    public void RegisterHealthBar(Transform target, float maxHealth, float heightOffset)
    {
        _healthBar = UIHealthBarController.Instance.RegisterHealthBar(target, maxHealth, heightOffset);
    }

    public void SetHealth(float health, float maxHealth)
    {
        _healthBar.SetHealth(health, maxHealth);
    }



    public void TakeDamage(float amount)
    {
        DamageResult result = DamageHelper.CalculateDamage(
            amount,
            CurrentStats.CritChance,
            CurrentStats.CritMultiplier,
            CurrentStats.Armor,
            CurrentStats.Evasion,
            CurrentStats.LifeStealRate
        );

        TakeDamage(result, transform.position);
    }

    public DamageResult DealDamage()
    {
        DamageResult result = DamageHelper.CalculateDamage(
            CurrentStats.Damage,
            CurrentStats.CritChance,
            CurrentStats.CritMultiplier,
            0, // target armor
            0, // target dodge
         CurrentStats.LifeStealRate
        );

        if (result.IsLifeSteal)
        {
            float healAmount = result.FinalDamage * CurrentStats.LifeSteal;
            Heal(healAmount);
        }
        return result;
    }

    public void Heal(float amount)
    {
        // CurrentHealth.Value = Mathf.Min(CurrentHealth.Value + amount, MaxHealth.Value);
        // SetHealth(CurrentHealth.Value, MaxHealth.Value);

        CurrentHP = Mathf.Min(CurrentHP + amount, CurrentStats.MaxHealth);
        UIDamageTextManager.Instance.ShowDamageText(transform.position, amount, TextType.Heal);

        var vfx = VFXPoolManager.Instance.GetEffect(VisualEffectID.Heal);
        vfx.transform.SetParent(transform);
        vfx.transform.localPosition = Vector3.zero;
        vfx.Play();
    }

    private void HandleRegen(float amount) // no show text
    {
        CurrentHP = Mathf.Min(CurrentHP + amount, CurrentStats.MaxHealth);
        SetHealth(CurrentHP, CurrentStats.MaxHealth);
    }


    public void UnregisterHealthBar()
    {
        UIHealthBarController.Instance.UnregisterHealthBar(_healthBar);
    }



    private void OnDestroy()
    {
    }

    public void TakeDamage(DamageResult result, Vector3 hitPoint)
    {
        if (result.IsMiss)
        {
            UIDamageTextManager.Instance.ShowDamageText(transform.position, 0, TextType.Miss);
            Debug.Log("Player dodged the attack!");
            return;
        }

        CurrentHP -= Mathf.Max(result.FinalDamage, 0);
        CurrentHP = Mathf.Max(CurrentHP, 0); // Ensure HP doesn't go below 0

        SetHealth(CurrentHP, CurrentStats.MaxHealth);
        
        // Invoke health changed event
        OnHealthChanged?.Invoke();

        UIDamageTextManager.Instance.ShowDamageText(
            transform.position,
            result.FinalDamage,
            result.IsCrit ? TextType.Critical : TextType.Normal
        );
        
        // Check for death
        CheckForDeath();
    }

    public float GetCurrentHealth() => CurrentHP;

    public float GetMaxHealth() => CurrentStats.MaxHealth;
    
    /// <summary>
    /// Check if player has died and trigger death event
    /// </summary>
    private void CheckForDeath()
    {
        if (!isDead && CurrentHP <= 0)
        {
            isDead = true;
            OnPlayerDeath?.Invoke();
            Debug.Log("Player has died!");
        }
    }
    
    /// <summary>
    /// Check if player is currently dead
    /// </summary>
    /// <returns>True if player is dead</returns>
    public bool IsDead()
    {
        return isDead;
    }
    
    /// <summary>
    /// Reset player health and death state (for restarting game)
    /// </summary>
    public void ResetHealth()
    {
        CurrentHP = CurrentStats.MaxHealth;
        isDead = false;
        SetHealth(CurrentHP, CurrentStats.MaxHealth);
        OnHealthChanged?.Invoke();
    }


    public void ChangeStat(StatType t, float value)
    {
        switch (t)
        {
            case StatType.Health:
                ModifiedStats.MaxHealth += value;
                CurrentHP += value;
                SetHealth(CurrentHP, CurrentStats.MaxHealth);
                break;
            case StatType.Damage:
                ModifiedStats.Damage += value;
                break;
            case StatType.CritChance:
                ModifiedStats.CritChance += value;
                break;
            case StatType.CritDamage:
                ModifiedStats.CritMultiplier += value;
                break;
            case StatType.LifeSteal:
                ModifiedStats.LifeSteal += value;
                break;
            case StatType.LifeStealRate:
                ModifiedStats.LifeStealRate += value;
                break;
            case StatType.Armor:
                ModifiedStats.Armor += value;
                break;
            case StatType.Evasion:
                ModifiedStats.Evasion += value;
                break;
            default:
                Debug.LogWarning($"Unhandled stat type: {t}");
                break;
        }
        OnStatChanged?.Invoke();
    }

}
[System.Serializable]
public struct CharacterStats
{
    public float MaxHealth;
    public float CurrentHealth;
    public float MoveSpeed;
    public float Damage;
    public float AttackSpeed;
    public float AttackRange;
    public float CritChance;
    public float CritMultiplier;
    public float Armor;
    public float Evasion;
    public float LifeSteal;
    public float LifeStealRate;
    public float HpRegen;
}