using UnityEngine;
using System.Collections.Generic;

public class WaveManager : MonoBehaviour
{
    public static WaveManager Instance { get; private set; }

    [Header("Wave Configuration")]
    public WaveData[] waves;
    public Transform[] enemySpawnPoints;

    [Header("Current State")]
    public int currentWave = 0;
    public bool isSpawning = false;

    private List<UnitController> currentWaveEnemies = new List<UnitController>();

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        if (waves == null || waves.Length == 0)
        {
            CreateDefaultWaves();
        }
    }

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
        currentWaveEnemies.Clear();

        Debug.Log($"第 {wave.waveNumber} 波敌人开始生成");

        Vector3[] spawnPositions = GetEnemySpawnPositions(wave);
        int spawnIndex = 0;

        foreach (EnemySpawnInfo spawnInfo in wave.enemies)
        {
            for (int i = 0; i < spawnInfo.count; i++)
            {
                Vector3 spawnPos = spawnPositions[spawnIndex % spawnPositions.Length];
                SpawnEnemy(spawnInfo.enemyData, spawnPos);
                spawnIndex++;
            }
        }

        isSpawning = false;
        Debug.Log($"第 {wave.waveNumber} 波敌人生成完毕，共 {currentWaveEnemies.Count} 个敌人");
    }

    Vector3[] GetEnemySpawnPositions(WaveData wave)
    {
        int totalEnemies = 0;
        foreach (EnemySpawnInfo spawnInfo in wave.enemies)
        {
            totalEnemies += spawnInfo.count;
        }

        Vector3[] positions = new Vector3[totalEnemies];

        int rows = Mathf.CeilToInt(Mathf.Sqrt(totalEnemies));
        int cols = Mathf.CeilToInt((float)totalEnemies / rows);

        for (int i = 0; i < totalEnemies; i++)
        {
            int row = i / cols;
            int col = i % cols;

            float x = 5f + (col * 1.5f);
            float y = (row - rows/2f) * 1.5f;

            positions[i] = new Vector3(x, y, 0f);
        }

        return positions;
    }

    void SpawnEnemy(UnitData enemyData, Vector3 position)
    {
        // 找到场景中的赤狐1或赤狐2作为模板
        GameObject foxTemplate = GameObject.Find("赤狐 1");
        if (foxTemplate == null)
        {
            foxTemplate = GameObject.Find("赤狐 2");
        }

        if (foxTemplate == null)
        {
            Debug.LogError("找不到赤狐1或赤狐2，无法生成敌人");
            return;
        }

        // 直接复制赤狐
        GameObject enemyInstance = Instantiate(foxTemplate, position, Quaternion.identity);
        UnitController enemyController = enemyInstance.GetComponent<UnitController>();

        if (enemyController != null)
        {
            // 设置为敌方单位
            enemyController.isEnemyTeam = true;
            enemyController.InitializeFromData();

            // 反转方向（让敌人朝左看）
            SpriteRenderer spriteRenderer = enemyInstance.GetComponent<SpriteRenderer>();
            if (spriteRenderer != null)
            {
                spriteRenderer.flipX = !spriteRenderer.flipX;
            }

            // 添加到敌人列表
            currentWaveEnemies.Add(enemyController);

            // 重命名
            enemyInstance.name = "敌方赤狐_" + currentWaveEnemies.Count;

            Debug.Log($"生成敌人：{enemyInstance.name} 在位置 {position}");
        }
        else
        {
            Debug.LogError("赤狐模板缺少UnitController组件");
            Destroy(enemyInstance);
        }
    }

    void Update()
    {
        if (!isSpawning && currentWaveEnemies.Count > 0)
        {
            currentWaveEnemies.RemoveAll(enemy => enemy == null);

            if (currentWaveEnemies.Count == 0)
            {
                GameManager.Instance.OnBattleWin();
            }
        }
    }

    public void CreateDefaultWaves()
    {
        waves = new WaveData[5];

        for (int i = 0; i < waves.Length; i++)
        {
            waves[i] = new WaveData();
            waves[i].waveNumber = i + 1;
            waves[i].spawnDelay = 0.5f;

            waves[i].enemies = new EnemySpawnInfo[1];
            waves[i].enemies[0] = new EnemySpawnInfo();
            waves[i].enemies[0].count = 1; // 每波都是1个敌人
            waves[i].enemies[0].spawnPosition = new Vector3(8f, 0f, 0f);
            waves[i].enemies[0].enemyData = null; // 不需要，直接复制场景中的赤狐
        }

        Debug.Log("创建了默认波次配置，将使用场景中的赤狐作为敌人");
    }

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
        return currentWave >= waves.Length - 1;
    }
}