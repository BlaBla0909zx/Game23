using UnityEngine;

[CreateAssetMenu(fileName = "EnemySO", menuName = "GameData/EnemySO")]
public class EnemySO : ScriptableObject
{
    public EnemyBase enemyPrefab;
    public EnemyID enemyID;
    public string enemyName;
    public float maxHealth;
    public float moveSpeed;
    public float meleeDamage;
    public float attackRange;
    public float attackSpeed;
    public float animationSpeed = 1.0f;
    public int coinDrop;

    [Tooltip("The height offset of the health bar")]
    public float healthBarHeightOffset = 1.5f;
}

public enum EnemyID
{
    Zombie = 0,
    Dummy = 99,
    Zombie_1 = 1,
}
