using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DebugManager : MonoBehaviour
{
    public static DebugManager Instance { get; private set; }

    [Header("调试设置")]
    public bool debugMode = true; // 调试模式开关
    public KeyCode debugKey = KeyCode.F1; // 按F1显示/隐藏调试面板

    [Header("调试UI")]
    public GameObject debugPanel; // 调试面板
    public Button testAIRecruitmentButton; // 测试AI招募官按钮
    public Button toggleDebugButton; // 切换调试模式按钮
    public TextMeshProUGUI debugInfoText; // 调试信息文本
    public TextMeshProUGUI friendshipInfoText; // 友谊值信息文本

    [Header("自动触发设置")]
    public bool autoTriggerOnGameStart = true; // 开始游戏时自动触发

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
        InitializeDebugUI();
        UpdateDebugInfo();
    }

    void Update()
    {
        // 按F1切换调试面板显示
        if (Input.GetKeyDown(debugKey))
        {
            ToggleDebugPanel();
        }

        // 定期更新调试信息
        if (Time.frameCount % 60 == 0) // 每秒更新一次
        {
            UpdateDebugInfo();
        }
    }

    void InitializeDebugUI()
    {
        // 绑定按钮事件
        if (testAIRecruitmentButton)
        {
            testAIRecruitmentButton.onClick.AddListener(OnTestAIRecruitment);
        }

        if (toggleDebugButton)
        {
            toggleDebugButton.onClick.AddListener(OnToggleDebugMode);
        }

        // 初始状态
        if (debugPanel)
        {
            debugPanel.SetActive(debugMode);
        }
    }

    public void ToggleDebugPanel()
    {
        if (debugPanel)
        {
            bool isActive = debugPanel.activeSelf;
            debugPanel.SetActive(!isActive);
            Debug.Log($"调试面板: {(!isActive ? "显示" : "隐藏")}");
        }
    }

    void OnTestAIRecruitment()
    {
        Debug.Log("[调试] 手动触发AI智能招募官");

        if (AIRecruitmentManager.Instance)
        {
            AIRecruitmentManager.Instance.TriggerAIRecruitment();
        }
        else
        {
            Debug.LogError("[调试] 找不到AIRecruitmentManager实例！");
        }
    }

    void OnToggleDebugMode()
    {
        debugMode = !debugMode;
        Debug.Log($"[调试] 调试模式: {(debugMode ? "开启" : "关闭")}");

        if (debugPanel)
        {
            debugPanel.SetActive(debugMode);
        }
    }

    void UpdateDebugInfo()
    {
        if (!debugMode) return;

        // 更新基本调试信息
        if (debugInfoText)
        {
            string info = $"调试模式: {(debugMode ? "开启" : "关闭")}\n";
            info += $"自动触发: {(autoTriggerOnGameStart ? "开启" : "关闭")}\n";
            info += $"AI招募官状态: {(AIRecruitmentManager.Instance ? "已加载" : "未加载")}\n";
            info += $"当前场景: {UnityEngine.SceneManagement.SceneManager.GetActiveScene().name}\n";
            info += $"按 {debugKey} 切换调试面板";

            debugInfoText.text = info;
        }

        // 更新友谊值信息
        if (friendshipInfoText && AIRecruitmentManager.Instance)
        {
            var friendships = AIRecruitmentManager.Instance.animalFriendships;
            string friendshipInfo = "=== 动物友谊值 ===\n";

            foreach (var pair in friendships)
            {
                string bonus = pair.Value.battleBonus > 0 ? $" (+{pair.Value.battleBonus:P0})" :
                              pair.Value.battleBonus < 0 ? $" ({pair.Value.battleBonus:P0})" : "";
                friendshipInfo += $"{pair.Key}: Lv.{pair.Value.friendshipLevel}{bonus}\n";
            }

            friendshipInfoText.text = friendshipInfo;
        }
    }

    // 供其他脚本调用的方法
    public void OnGameStarted()
    {
        if (autoTriggerOnGameStart)
        {
            Debug.Log("[调试] 游戏开始，自动触发AI招募官");
            // 延迟1秒触发，确保场景加载完成
            Invoke("TriggerAIRecruitmentDelayed", 1f);
        }
    }

    void TriggerAIRecruitmentDelayed()
    {
        OnTestAIRecruitment();
    }

    // 供Unity Inspector调用的测试方法
    [ContextMenu("测试AI招募官")]
    public void TestAIRecruitmentFromInspector()
    {
        OnTestAIRecruitment();
    }

    [ContextMenu("重置所有友谊值")]
    public void ResetAllFriendships()
    {
        if (AIRecruitmentManager.Instance)
        {
            foreach (var friendship in AIRecruitmentManager.Instance.animalFriendships.Values)
            {
                friendship.friendshipLevel = 0;
                friendship.battleBonus = 0f;
            }
            Debug.Log("[调试] 已重置所有友谊值");
        }
    }
}