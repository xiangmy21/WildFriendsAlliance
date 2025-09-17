using UnityEngine;

[System.Serializable]
public class WaveData
{
    [Header("Wave Info")]
    public int waveNumber;
    public float spawnDelay = 0.5f; // 敌人生成间隔

    [Header("Enemy Spawns")]
    public EnemySpawnInfo[] enemies;
}