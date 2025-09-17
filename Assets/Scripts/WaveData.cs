using UnityEngine;

public class WaveData : MonoBehaviour
{
    [Header("Wave Info")]
    public float spawnDelay = 1f; // 敌人生成间隔

    [Header("Enemy Spawns")]
    public EnemySpawnInfo[] enemies;
}