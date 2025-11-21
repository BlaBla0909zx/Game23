using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GameDataManager : MonoBehaviour
{
    public static GameDataManager Instance { get; private set; }

    public Dictionary<VisualEffectID, BaseVisualEffect> VisualEffectDictionary { get; private set; }

    //rarity color
    public Dictionary<Rarity, Color> RarityColorDictionary =
        new()
        {
            { Rarity.Common, Color.white },
            { Rarity.Rare, Color.green },
            { Rarity.Epic, Color.blue },
            {
                Rarity.Legendary,
                new Color(1f, 0.5f, 0f)
            } // orange
            ,
        };

    public void GetRarityColor(Rarity rarity, out Color color)
    {
        if (RarityColorDictionary.TryGetValue(rarity, out color))
        {
            return;
        }
        color = Color.white;
    }

    //load skillSO
    public List<AbilitiesSO> AllSkills = new();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
            return;
        }
        Instance = this;
        LoadAllVisualEffect();
        LoadSkillData();
    }

    private void Start() { }

    private void LoadSkillData()
    {
        AbilitiesSO[] skillSOs = Resources.LoadAll<AbilitiesSO>("AbilitiesSO/");
        AllSkills = skillSOs.ToList();
    }

    private void LoadAllVisualEffect()
    {
        BaseVisualEffect[] vfxs = Resources.LoadAll<BaseVisualEffect>("Effects/");
        VisualEffectDictionary = new();
        for (int i = 0; i < vfxs.Length; i++)
        {
            BaseVisualEffect vfx = vfxs[i];
            vfx.Initialize();
            VisualEffectDictionary.Add(vfx.VfxID, vfx);
        }
    }
}
