using UnityEngine;
using System.Collections.Generic;

public class HealingArea : MonoBehaviour, IAreaEffect
{
    [Header("Healing Settings")]
    public float healingRadius = 1.6f;
    public LayerMask playerLayer;

    private List<EffectInstance> effectInstances = new List<EffectInstance>();
    private bool isEffectRunning = false;

    public bool IsRunning => isEffectRunning && effectInstances.Count > 0;



    public void Initialize(AreaEffectConfig[] configs)
    {
        effectInstances.Clear();

        foreach (var config in configs)
        {
            effectInstances.Add(new EffectInstance(config));
        }

        isEffectRunning = true;
    }

    public void Stop()
    {
        isEffectRunning = false;
        effectInstances.Clear();
    }

    private void Update()
    {
        if (!isEffectRunning) return;

        bool anyEffectActive = false;

        for (int i = effectInstances.Count - 1; i >= 0; i--)
        {
            var effect = effectInstances[i];

            if (effect.isFinished) continue;

            // Handle activation delay
            if (!effect.isActive)
            {
                effect.remainingActivationDelay -= Time.deltaTime;
                if (effect.remainingActivationDelay <= 0f)
                {
                    effect.isActive = true;

                    // For OneShot, heal immediately and mark as finished
                    if (effect.config.IsOneShot)
                    {
                        CheckAndHealPlayer(effect.config.damage);
                        effect.isFinished = true;
                        continue;
                    }
                }
                else
                {
                    anyEffectActive = true;
                    continue;
                }
            }

            // Handle continuous effects
            if (effect.config.IsContinuous && effect.isActive)
            {
                effect.timer += Time.deltaTime;
                effect.remainingDuration -= Time.deltaTime;

                // Heal at intervals
                if (effect.timer >= effect.config.interval)
                {
                    effect.timer = 0f;
                    CheckAndHealPlayer(effect.config.damage);
                }

                // Check if duration expired
                if (effect.remainingDuration <= 0f)
                {
                    effect.isFinished = true;
                }
                else
                {
                    anyEffectActive = true;
                }
            }
        }

        // Remove finished effects
        effectInstances.RemoveAll(e => e.isFinished);

        // Stop if no effects are running
        if (!anyEffectActive && effectInstances.Count == 0)
        {
            Stop();
        }
    }

    private void CheckAndHealPlayer(float amount)
    {
        if (PlayerStats.Instance != null)
        {
            float distanceToPlayer = Vector3.Distance(transform.position, PlayerStats.Instance.transform.position);

            if (distanceToPlayer <= healingRadius)
            {
                PlayerStats.Instance.Heal(amount);
            }
        }
    }
}