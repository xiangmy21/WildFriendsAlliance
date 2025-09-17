using UnityEngine;
using UnityEngine.UI;

public class GameUIController : MonoBehaviour
{
    public static GameUIController Instance { get; private set; }

    [Header("UI按钮")]
    public Button triggerAIRecruitmentButton;
    public Button endWaveButton; // 模拟波次结束

    [Header("调试信息")]
    public TMPro.TextMeshProUGUI debugText;

    [Header("牌库显示")]
    public TMPro.TextMeshProUGUI deckInfoText; // 牌库状态显示文本

    void Awake()
    {
        // 单例模式
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        // 绑定按钮事件
        if (triggerAIRecruitmentButton)
        {
            triggerAIRecruitmentButton.onClick.AddListener(OnTriggerAIRecruitment);
        }

        if (endWaveButton)
        {
            endWaveButton.onClick.AddListener(OnEndWave);
        }

        UpdateDebugInfo();
        UpdateDeckInfo(); // 初始化牌库信息显示
    }

    void OnTriggerAIRecruitment()
    {
        Debug.Log("手动触发AI智能招募官");

        if (AIRecruitmentManager.Instance)
        {
            AIRecruitmentManager.Instance.TriggerAIRecruitment();
        }
        else
        {
            Debug.LogError("找不到AIRecruitmentManager实例！");
        }
    }

void OnEndWave()
    {
        Debug.Log("模拟波次结束，自动触发AI智能招募官");

        // 模拟波次结束逻辑
        if (GameManager.Instance)
        {
            // 使用新的方法：模拟战斗胜利
            GameManager.Instance.OnBattleWin();
        }

        // 触发AI招募官
        OnTriggerAIRecruitment();
    }

    void UpdateDebugInfo()
    {
        if (debugText && AIRecruitmentManager.Instance)
        {
            var friendships = AIRecruitmentManager.Instance.animalFriendships;
            string info = "动物友谊值:\n";

            foreach (var pair in friendships)
            {
                info += $"{pair.Key}: Lv.{pair.Value.friendshipLevel} (奖励:{pair.Value.battleBonus:P0})\n";
            }

            debugText.text = info;
        }
    }

    // 定期更新调试信息
    void Update()
    {
        if (Time.frameCount % 60 == 0) // 每秒更新一次
        {
            UpdateDebugInfo();
            UpdateDeckInfo();
        }
    }

    // 更新牌库信息显示
    public void UpdateDeckInfo()
    {
        if (deckInfoText && AIRecruitmentManager.Instance != null)
        {
            var deckData = AIRecruitmentManager.Instance.GetDeckCopy();
            string deckInfo = "牌库状态:\n";

            bool hasCards = false;
            foreach (var pair in deckData)
            {
                if (pair.Value > 0)
                {
                    deckInfo += $"{pair.Key}: x{pair.Value}\n";
                    hasCards = true;
                }
            }

            if (!hasCards)
            {
                deckInfo += "牌库为空\n";
            }

            // 计算总卡牌数
            int totalCards = 0;
            foreach (var pair in deckData)
            {
                totalCards += pair.Value;
            }
            deckInfo += $"总计: {totalCards}张卡牌";

            deckInfoText.text = deckInfo;
        }
        else if (deckInfoText)
        {
            deckInfoText.text = "牌库状态:\n等待初始化...";
        }
    }
}