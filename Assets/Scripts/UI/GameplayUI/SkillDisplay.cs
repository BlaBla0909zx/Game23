using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SkillDisplay : MonoBehaviour
{
    [SerializeField] private Image icon;

    private AbilitiesSO skillData;
    private Material iconMaterial; // Material instance for shader control

    public void Initialize(AbilitiesSO skillSO)
    {
        skillData = skillSO;
        icon.sprite = skillSO.skillIcon;

        // Create material instance
        if (icon.material != null)
        {
            iconMaterial = new Material(icon.material);
            icon.material = iconMaterial;
        }

        // Initialize cooldown display
        UpdateCooldownDisplay(0f);
    }

    public void UpdateCooldownDisplay(float currentCooldown)
    {
        bool isOnCooldown = currentCooldown > 0f;
        // Update shader progress based on cooldown
        UpdateShaderProgress(currentCooldown);

        // Update visual state
        UpdateVisualState(isOnCooldown);
    }

    private void UpdateShaderProgress(float currentCooldown)
    {
        if (iconMaterial != null && skillData != null && skillData.cooldown > 0)
        {
            // Calculate progress (0 = just used, 1 = ready to use)
            float progress = 1f - (currentCooldown / skillData.cooldown);
            progress = Mathf.Clamp01(progress);

            // Set shader properties
            iconMaterial.SetFloat("_ShinyProgress", progress);
            iconMaterial.SetFloat("_ShinyEnabled", currentCooldown > 0 ? 1f : 0f);
        }
    }

    private void UpdateVisualState(bool isOnCooldown)
    {
        // Dim the icon when on cooldown
        Color iconColor = icon.color;
        iconColor.a = isOnCooldown ? 0.5f : 1f;
        icon.color = iconColor;
    }

    private void OnDestroy()
    {
        // Clean up material instance to prevent memory leaks
        if (iconMaterial != null)
        {
            DestroyImmediate(iconMaterial);
        }
    }
}