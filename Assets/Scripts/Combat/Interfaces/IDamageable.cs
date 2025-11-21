using UnityEngine;
public interface IDamageable
{
    void TakeDamage(DamageResult result, Vector3 hitPoint);
    void Heal(float amount);
    float GetCurrentHealth();
    float GetMaxHealth();
}