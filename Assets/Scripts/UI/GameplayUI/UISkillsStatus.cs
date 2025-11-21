using System.Collections.Generic;
using UnityEngine;

public class UISkillsStatus : MonoBehaviour
{
    [SerializeField] private SkillDisplay skillDisplayPrefab;
    private List<SkillDisplay> skillDisplays = new();
    [SerializeField] private Transform skillDisplayContainer;


    private void Start()
    {
        PlayerStats.Instance.OnDataInit += InitializeSkillDisplays;
    }

    private void InitializeSkillDisplays()
    {
        // Clear existing displays
        ClearSkillDisplays();

        for (int i = 0; i < PlayerStats.Instance.SkillInstances.Count; i++)
        {
            Skill_Base skill = PlayerStats.Instance.SkillInstances[i];
            SkillDisplay skillDisplay = Instantiate(skillDisplayPrefab, skillDisplayContainer);
            skillDisplay.Initialize(skill.SkillData);
            skillDisplays.Add(skillDisplay);

            // Capture index for closure
            int skillIndex = i;
            skill.CooldownTimer.Subscribe((oldValue, newValue) =>
                UpdateSkillCooldownDisplay(skillIndex, newValue));
        }
    }

    private void UpdateSkillCooldownDisplay(int skillIndex, float cooldownValue)
    {
        if (skillIndex >= 0 && skillIndex < skillDisplays.Count)
        {
            skillDisplays[skillIndex].UpdateCooldownDisplay(cooldownValue);
        }
    }

    private void ClearSkillDisplays()
    {
        foreach (var display in skillDisplays)
        {
            if (display != null)
                DestroyImmediate(display.gameObject);
        }
        skillDisplays.Clear();
    }

    private void OnDestroy()
    {
        if (PlayerStats.Instance != null)
        {
            PlayerStats.Instance.OnDataInit -= InitializeSkillDisplays;
        }
    }
}