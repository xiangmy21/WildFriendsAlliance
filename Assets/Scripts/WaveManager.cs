using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class WaveManager : MonoBehaviour
{
    public static WaveManager Instance { get; private set; }

    [Header("Wave Configuration")]
    public WaveData[] waves;

    [Header("Current State")]
    public int currentWave = 0; // 当前波次索引(从0开始)
    public bool isSpawning = false;
    private int spawnCount = 0;

    private List<UnitController> currentWaveEnemies = new List<UnitController>();

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    //void Start()
    //{
    //    if (waves == null || waves.Length == 0)
    //    {
    //        CreateDefaultWaves();
    //    }
    //}

    public void SpawnWave(int waveIndex)
    {
        if (isSpawning)
        {
            Debug.LogWarning("已经在生成敌人中，无法重复生成");
            return;
        }

        if (waveIndex >= waves.Length)
        {
            Debug.Log("所有波次已完成！游戏胜利！");
            GameManager.Instance.OnGameVictory();
            return;
        }

        currentWave = waveIndex;
        SpawnAllEnemiesForWave(waves[waveIndex]);
    }

    void SpawnAllEnemiesForWave(WaveData wave)
    {
        isSpawning = true;
        spawnCount = 0;
        currentWaveEnemies.Clear();

        Debug.Log($"第 {currentWave+1} 波敌人开始生成");

        StartCoroutine(SpawnEnemiesWithDelay(wave));
    }

    IEnumerator SpawnEnemiesWithDelay(WaveData wave)
    {
        for (int i = 0; i < wave.enemies.Length; i++)
        {
            // 第一个敌人无延迟
            if (i > 0)
            {
                yield return new WaitForSeconds(wave.spawnDelay);
            }

            SpawnEnemy(wave.enemies[i].enemyData, wave.enemies[i].spawnPosition);
        }

        isSpawning = false;
        Debug.Log($"第 {currentWave+1} 波敌人生成完毕，共 {wave.enemies.Length} 个敌人");
    }

    void SpawnEnemy(UnitData enemyData, Transform position)
    {
        if (enemyData == null || enemyData.unitPrefab == null)
        {
            Debug.LogError("EnemyData 或 prefab 为空，无法生成实例！");
            return;
        }

        GameObject enemyInstance = Instantiate(enemyData.unitPrefab, position.position, Quaternion.identity);
        UnitController enemyController = enemyInstance.GetComponent<UnitController>();

        // 设置为敌方单位
        enemyController.isEnemyTeam = true;

        // 反转方向（让敌人朝左看）
        SpriteRenderer spriteRenderer = enemyInstance.GetComponent<SpriteRenderer>();
        spriteRenderer.flipX = true;

        // 添加到敌人列表
        currentWaveEnemies.Add(enemyController);
        spawnCount++;

        // 重命名
        enemyInstance.name = enemyData.name;

        Debug.Log($"生成敌人：{enemyInstance.name} 在位置 {position}");
    }

    void Update()
    {
        if(GameManager.Instance.CurrentState == GameManager.GameState.Battle)
        {
            currentWaveEnemies.RemoveAll(enemy => enemy == null);

            if (spawnCount == waves[currentWave].enemies.Length && currentWaveEnemies.Count == 0)
            {
                GameManager.Instance.OnBattleWin();
            }
        }
    }

    //public void CreateDefaultWaves()
    //{
    //    waves = new WaveData[5];

    //    for (int i = 0; i < waves.Length; i++)
    //    {
    //        waves[i] = new WaveData();
    //        waves[i].waveNumber = i + 1;
    //        waves[i].spawnDelay = 0.5f;

    //        waves[i].enemies = new EnemySpawnInfo[1];
    //        waves[i].enemies[0] = new EnemySpawnInfo();
    //        waves[i].enemies[0].count = 1; // 每波都是1个敌人
    //        waves[i].enemies[0].spawnPosition = new Vector3(8f, 0f, 0f);
    //        waves[i].enemies[0].enemyData = null; // 不需要，直接复制场景中的赤狐
    //    }

    //    Debug.Log("创建了默认波次配置，将使用场景中的赤狐作为敌人");
    //}

    public int GetCurrentWave()
    {
        return currentWave + 1;
    }

    public int GetTotalWaves()
    {
        return waves != null ? waves.Length : 0;
    }

public bool IsLastWave()
    {
        // 使用GameManager的可配置波数进行判定
        if (GameManager.Instance != null)
        {
            bool isLast = currentWave >= GameManager.Instance.totalWaves - 1;
            Debug.Log($"[波次判定] 当前波次:{currentWave}, 总波数:{GameManager.Instance.totalWaves}, 是否最后一波:{isLast}");
            return isLast;
        }
        
        // 备用方案：如果GameManager不存在，使用waves数组长度
        Debug.LogWarning("[波次判定] GameManager不存在，使用默认waves数组判定");
        return currentWave >= waves.Length - 1;
    }
}