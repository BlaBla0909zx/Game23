using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(Collider))]
public class DealDamageArea : MonoBehaviour, IAreaEffect
{
    [Header("Damage Settings")]
    public LayerMask enemyLayer;
    private Collider hitboxCollider;

    private List<EffectInstance> effectInstances = new List<EffectInstance>();
    private bool isEffectRunning = false;

    public bool IsRunning => isEffectRunning && effectInstances.Count > 0;



    public void Initialize(AreaEffectConfig[] configs)
    {
        hitboxCollider = GetComponent<Collider>();
        hitboxCollider.isTrigger = true;

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
                    if (effect.config.IsOneShot)
                    {
                        DealAreaDamage(effect.config.damage);
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

                // Deal damage at intervals
                if (effect.timer >= effect.config.interval)
                {
                    effect.timer = 0f;
                    DealAreaDamage(effect.config.damage);
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

    private void DealAreaDamage(float damage)
    {
        Vector3 center = hitboxCollider.bounds.center;
        float radius = GetEffectiveRadius();

        Collider[] hitTargets = Physics.OverlapSphere(center, radius, enemyLayer);

        foreach (var targetCollider in hitTargets)
        {
            if (targetCollider.TryGetComponent<IDamageable>(out var damageable))
            {
                DamageResult damageResult = new DamageResult { FinalDamage = damage * UnityEngine.Random.Range(0.8f, 1f) };
                Vector3 hitPoint = targetCollider.ClosestPoint(center);
                damageable.TakeDamage(damageResult, hitPoint);
            }
        }
    }

    private float GetEffectiveRadius()
    {
        return Mathf.Max(hitboxCollider.bounds.extents.x,
                        hitboxCollider.bounds.extents.y,
                        hitboxCollider.bounds.extents.z);
    }

}
[System.Serializable]
public class EffectInstance
{
    public AreaEffectConfig config;
    public float remainingActivationDelay;
    public float remainingDuration;
    public float timer;
    public bool isActive;
    public bool isFinished;

    public EffectInstance(AreaEffectConfig config)
    {
        this.config = config;
        this.remainingActivationDelay = config.activationDelay;
        this.remainingDuration = config.IsContinuous ? config.duration : 0f;
        this.timer = 0f;
        this.isActive = false;
        this.isFinished = false;
    }
}