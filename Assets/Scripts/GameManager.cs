using UnityEngine;

public class GameManager : MonoBehaviour
{
    // --- 这是单例的核心 ---

    // "Instance" 是一个静态变量，意味着它不属于任何一个单独的GameManager，
    // 而是属于 "GameManager" 这个“类”本身。
    // 任何脚本都可以通过 GameManager.Instance 来访问它。
    

    [Header("游戏配置")]
    [Tooltip("总波数：调试时设为1，正式版本设为10")]
    public int totalWaves = 1; // 调试阶段默认为1波
public static GameManager Instance { get; private set; }

    void Awake()
    {
        // 这段代码是“单例模式”的经典写法
        if (Instance == null)
        {
            // 如果 Instance 还没有被赋值 (说明这是第一个GameManager)
            Instance = this; // 就把 "this" (也就是当前这个脚本组件) 赋给它

            // (可选) 这行代码让你的GameManager在切换场景时不会被销毁
            // 在黑客松里，如果你有 "主菜单" 和 "战斗" 两个场景，这就很有用
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            // 如果 Instance 已经被赋值了 (说明场景里已经有一个GameManager了)
            // 就把自己销毁，保证永远只有一个
            Destroy(gameObject);
        }
    }

    // --- 游戏状态管理 ---

    public enum GameState { Preparation, Battle, GameOver, Victory }
    public GameState CurrentState { get; private set; } = GameState.Preparation;

    // 兼容旧代码的属性
    public bool IsBattleActive => CurrentState == GameState.Battle;

    // 开始战斗
    public void StartBattle()
    {
        if (CurrentState != GameState.Preparation)
        {
            Debug.LogWarning("当前不在准备阶段，无法开始战斗");
            return;
        }

        CurrentState = GameState.Battle;

        // 通知WaveManager开始生成敌人
        if (WaveManager.Instance != null)
        {
            int currentWaveIndex = WaveManager.Instance.currentWave;
            Debug.Log($"第 {currentWaveIndex + 1} 波战斗开始！");
            WaveManager.Instance.SpawnWave(currentWaveIndex);
        }
        else
        {
            Debug.LogError("WaveManager未找到！");
        }
    }

    // 战斗胜利（当前波次敌人全部击败）
public void OnBattleWin()
    {
        if (CurrentState != GameState.Battle) return;

        int currentWaveIndex = WaveManager.Instance != null ? WaveManager.Instance.currentWave : 0;
        Debug.Log($"第 {currentWaveIndex + 1} 波战斗胜利！");

        // 奖励金币
        AddGold(3);

        // 检查是否为最终胜利（所有波次完成）
        if (currentWaveIndex >= totalWaves - 1)
        {
            Debug.Log($"[最终胜利] 已完成所有{totalWaves}波，游戏胜利！");
            OnGameVictory();
            return;
        }

        // 如果不是最后一波，进入下一波的准备阶段
        if (WaveManager.Instance != null)
        {
            WaveManager.Instance.currentWave++;
        }
        OnBattleEnd(true);
    }

    // 战斗失败（玩家单位全部阵亡）
    public void OnBattleLose()
    {
        if (CurrentState != GameState.Battle) return;

        int currentWaveIndex = WaveManager.Instance != null ? WaveManager.Instance.currentWave : 0;
        Debug.Log($"第 {currentWaveIndex + 1} 波战斗失败！");

        // 扣除生命值
        TakePlayerDamage(1);

        if (PlayerHealth <= 0)
        {
            OnGameOver();
        }
        else
        {
            // 重试当前波次 - 不增加波次计数
            OnBattleEnd(false);
        }
    }

    // 战斗结束，回到准备阶段
public void OnBattleEnd(bool playerWon)
    {
        CurrentState = GameState.Preparation;
        int currentWaveIndex = WaveManager.Instance != null ? WaveManager.Instance.currentWave : 0;
        Debug.Log($"战斗结束，回到准备阶段。当前波次：{currentWaveIndex + 1}");

        // 如果玩家胜利，增加金币
        if (playerWon)
        {
            AddGold(3);
            Debug.Log("获得奖励3金币");
        }

        // 每波结束后都触发AI问答系统
        Debug.Log("[调试] 开始检查AIRecruitmentManager");
        if (AIRecruitmentManager.Instance != null)
        {
            Debug.Log($"[成功] 第{currentWaveIndex + 1}波结束，触发AI智能招募官");
            AIRecruitmentManager.Instance.TriggerAIRecruitment();
            return; // 等待问答完成，不立即更新UI
        }
        else
        {
            Debug.LogError("[错误] AIRecruitmentManager.Instance 为 null，跳过AI问答");
        }

        // TODO: 刷新商店
        
        // 显示下一波按钮
        UpdateUIForNextWave();
    }

    void UpdateUIForNextWave()
    {
        // 通知UI更新按钮文字，显示"进入下一波"或"开始对战"
        int currentWaveIndex = WaveManager.Instance != null ? WaveManager.Instance.currentWave : 0;
        Debug.Log($"准备阶段，可以购买单位和开始第{currentWaveIndex + 1}波战斗");
    }


    // 游戏胜利
public void OnGameVictory()
    {
        CurrentState = GameState.Victory;
        Debug.Log($"[游戏胜利] 恭喜！成功完成全部{totalWaves}波挑战！");
        
        // 查找并显示胜利UI
        VictoryUI victoryUI = FindObjectOfType<VictoryUI>();
        if (victoryUI != null)
        {
            victoryUI.ShowVictory();
        }
        else
        {
            Debug.Log("[游戏胜利] 未找到VictoryUI，创建临时胜利界面");
            // 创建临时VictoryUI对象
            GameObject victoryObj = new GameObject("TempVictoryUI");
            VictoryUI tempVictoryUI = victoryObj.AddComponent<VictoryUI>();
            tempVictoryUI.ShowVictory();
        }
    }

    // AI问答系统完成后的回调
    public void OnQuizCompleted()
    {
        Debug.Log("AI问答完成，继续游戏流程");
        
        // TODO: 刷新商店
        
        // 显示下一波按钮
        UpdateUIForNextWave();
    }


    // 游戏失败
    public void OnGameOver()
    {
        CurrentState = GameState.GameOver;
        Debug.Log("游戏失败！");
        // TODO: 显示失败界面
    }

    // --- 游戏的其他全局数据 ---

    // 你可以把金币、玩家血量等也放在这里
    public int PlayerHealth = 5;
    public int PlayerGold = 10;

    // (示例)
    public void TakePlayerDamage(int damage)
    {
        PlayerHealth -= damage;
        if (PlayerHealth <= 0)
        {
            Debug.Log("游戏失败！");
            // TODO: 显示失败界面
        }
    }

    public void SpendGold(int gold)
    {
        PlayerGold -= gold;
    }
    public void AddGold(int gold)
    {
        PlayerGold += gold;
    }

    // 检查玩家是否还有存活的单位
    void Update()
    {
        if (CurrentState == GameState.Battle)
        {
            CheckBattleStatus();
        }
    }

    void CheckBattleStatus()
    {
        // 查找场景中所有的UnitController
        UnitController[] allUnits = FindObjectsOfType<UnitController>();

        bool hasPlayerUnits = false;
        bool hasEnemyUnits = false;

        foreach (UnitController unit in allUnits)
        {
            // 跳过已死亡的单位
            if (unit == null) continue;

            if (unit.isEnemyTeam)
            {
                hasEnemyUnits = true;
            }
            else
            {
                hasPlayerUnits = true;
            }
        }

        // 如果玩家没有单位了，战斗失败
        if (!hasPlayerUnits)
        {
            OnBattleLose();
        }
        // 如果敌人没有单位了，WaveManager会自动调用OnBattleWin
    }
}