using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(menuName = "Wave/EnemyWave")]
public class EnemyWaveSO : ScriptableObject
{
    public List<EnemyEntry> enemies;
}

[System.Serializable]
public class EnemyEntry
{
    public EnemySO enemyData;
    public int enemyCount = 5;
    public float spawnRate = 1f;      // delay giữa từng con cùng loại
    public float startDelay = 0f;     // delay trước khi spawn loại này
}