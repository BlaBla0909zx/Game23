using UnityEngine;

public struct DamageResult
{
    public float FinalDamage;
    public bool IsCrit;
    public bool IsMiss;
    public bool IsLifeSteal; // thêm trường này để đánh dấu nếu là sát thương hồi máu
}

public static class DamageHelper
{
    /// <summary>
    /// Tính toán damage với crit, giáp, dodge.
    /// </summary>
    public static DamageResult CalculateDamage(
        float baseDamage,
        float critChance,     // 0-1
        float critMultiplier, // vd: 2.0 = x2 damage
        float armor,          // % giảm damage (0-100)
        float dodgeChance,    // 0-1
        float lifeStealRate = -1f // 0-1, only player have
    )
    {
        DamageResult result = new DamageResult();

        // 1. Check dodge
        if (Random.value < dodgeChance)
        {
            result.IsMiss = true;
            result.FinalDamage = 0;
            return result;
        }

        float damage = Random.Range(baseDamage * 0.7f, baseDamage * 1.1f);

        // 2. Check crit    
        if (Random.value < critChance)
        {
            damage *= critMultiplier;
            result.IsCrit = true;
        }

        // 3. Apply armor reduction
        float reduced = damage * (1f - Mathf.Clamp01(armor / 100f));
        result.FinalDamage = Mathf.Max(0, reduced);
        result.IsLifeSteal = false;
        if (lifeStealRate != 1)
        {
            float roll = Random.value;
            if (roll < lifeStealRate)
            {
                result.IsLifeSteal = true;
            }
        }

        return result;
    }
}