using UnityEngine;

public class GameManager : MonoBehaviour
{
    // --- 这是单例的核心 ---

    // "Instance" 是一个静态变量，意味着它不属于任何一个单独的GameManager，
    // 而是属于 "GameManager" 这个“类”本身。
    // 任何脚本都可以通过 GameManager.Instance 来访问它。
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

    // 这就是你的全局状态，我们希望它能被所有人读取 (public get)，
    // 但只能被 GameManager 自己修改 (private set)。
    public bool IsBattleActive { get; private set; }

    // 你需要提供“公共方法”来让其他脚本（比如UI按钮）来改变这个状态
    public void StartBattle()
    {
        IsBattleActive = true;
        Debug.Log("战斗开始！");

        // TODO: 在这里通知你的 WaveManager 开始刷怪
        // 比如: WaveManager.Instance.SpawnWave(currentWave);
    }

    public void EndBattle()
    {
        IsBattleActive = false;
        Debug.Log("战斗结束！");

        // TODO: 在这里处理战斗结束的结算逻辑
        // 比如: UIManager.Instance.ShowVictoryScreen();
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
}