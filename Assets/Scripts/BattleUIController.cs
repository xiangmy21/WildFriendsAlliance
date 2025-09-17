using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BattleUIController : MonoBehaviour
{
    [Header("UI References")]
    public Button startBattleButton;
    public TextMeshProUGUI waveDisplayText;
    public TextMeshProUGUI goldDisplayText;
    // public TextMeshProUGUI healthDisplayText; // removed
    public TextMeshProUGUI gameStateText;

    void Start()
    {
        // 绑定按钮事件
        if (startBattleButton != null)
        {
            startBattleButton.onClick.AddListener(OnStartBattleClicked);
        }

        // 初始化UI显示
        UpdateUI();
    }

    void Update()
    {
        // 每帧更新UI显示
        UpdateUI();
    }

    public void OnStartBattleClicked()
    {
        Debug.Log("开始对战按钮被点击");

        if (GameManager.Instance != null)
        {
            GameManager.Instance.StartBattle();
        }
        else
        {
            Debug.LogError("GameManager未找到！");
        }
    }

void UpdateUI()
    {
        if (GameManager.Instance == null) return;

        // 更新波次显示
        if (waveDisplayText != null && WaveManager.Instance != null)
        {
            int currentWave = WaveManager.Instance.GetCurrentWave();
            int totalWaves = WaveManager.Instance.GetTotalWaves();
            waveDisplayText.text = $"波次: {currentWave}/{totalWaves}";
        }

        // 更新金币显示
        if (goldDisplayText != null)
        {
            goldDisplayText.text = $"金币: {GameManager.Instance.PlayerGold}";
        }

        // 更新生命值显示
        
        // 更新游戏状态显示
        if (gameStateText != null)
        {
            gameStateText.text = $"状态: {GetStateDisplayText(GameManager.Instance.CurrentState)}";
        }

        // 更新按钮状态
        if (startBattleButton != null)
        {
            // 只有在准备阶段才能开始战斗
            startBattleButton.interactable = (GameManager.Instance.CurrentState == GameManager.GameState.Preparation);

            // 根据状态更改按钮文字
            var buttonText = startBattleButton.GetComponentInChildren<TMPro.TextMeshProUGUI>();
            if (buttonText != null && WaveManager.Instance != null)
            {
                switch (GameManager.Instance.CurrentState)
                {
                    case GameManager.GameState.Preparation:
                        int nextWave = WaveManager.Instance.currentWave + 1;
                        buttonText.text = $"开始第{nextWave}波";
                        break;
                    case GameManager.GameState.Battle:
                        buttonText.text = "战斗中...";
                        break;
                    case GameManager.GameState.Victory:
                        buttonText.text = "游戏胜利";
                        break;
                    case GameManager.GameState.GameOver:
                        buttonText.text = "游戏失败";
                        break;
                }
            }
        }
    }

    string GetStateDisplayText(GameManager.GameState state)
    {
        switch (state)
        {
            case GameManager.GameState.Preparation:
                return "准备阶段";
            case GameManager.GameState.Battle:
                return "战斗中";
            case GameManager.GameState.Victory:
                return "胜利";
            case GameManager.GameState.GameOver:
                return "失败";
            default:
                return "未知";
        }
    }
}