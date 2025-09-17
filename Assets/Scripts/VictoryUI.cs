using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class VictoryUI : MonoBehaviour
{
    [Header("UI引用")]
    public GameObject victoryPanel;
    public Button returnHomeButton;
    public Text victoryText; // 可选：显示胜利信息

    void Start()
    {
        // 初始状态隐藏胜利界面
        if (victoryPanel) victoryPanel.SetActive(false);
        
        // 绑定返回按钮事件
        if (returnHomeButton)
        {
            returnHomeButton.onClick.AddListener(ReturnToHomePage);
        }
    }

    public void ShowVictory()
    {
        Debug.Log("[胜利UI] 显示游戏胜利界面");
        
        if (victoryPanel)
        {
            victoryPanel.SetActive(true);
            
            // 更新胜利文本
            if (victoryText)
            {
                int totalWaves = GameManager.Instance ? GameManager.Instance.totalWaves : 1;
                victoryText.text = $"恭喜！\n成功完成全部 {totalWaves} 波挑战！";
            }
        }
        else
        {
            Debug.LogWarning("[胜利UI] victoryPanel未设置，创建简单弹窗");
            CreateSimpleVictoryDialog();
        }
    }

    void CreateSimpleVictoryDialog()
    {
        // 如果没有配置UI，创建一个简单的代码生成弹窗
        GameObject dialog = new GameObject("VictoryDialog");
        Canvas canvas = dialog.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;
        
        dialog.AddComponent<GraphicRaycaster>();
        
        // 创建背景
        GameObject background = new GameObject("Background");
        background.transform.SetParent(dialog.transform, false);
        Image bgImage = background.AddComponent<Image>();
        bgImage.color = new Color(0, 0, 0, 0.8f);
        
        RectTransform bgRect = background.GetComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.sizeDelta = Vector2.zero;
        
        // 创建胜利文本
        GameObject textObj = new GameObject("VictoryText");
        textObj.transform.SetParent(dialog.transform, false);
        Text text = textObj.AddComponent<Text>();
        text.text = $"恭喜通关！\n完成全部 {(GameManager.Instance ? GameManager.Instance.totalWaves : 1)} 波挑战！";
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.fontSize = 24;
        text.color = Color.white;
        text.alignment = TextAnchor.MiddleCenter;
        
        RectTransform textRect = textObj.GetComponent<RectTransform>();
        textRect.anchoredPosition = new Vector2(0, 50);
        textRect.sizeDelta = new Vector2(400, 100);
        
        // 创建返回按钮
        GameObject buttonObj = new GameObject("ReturnButton");
        buttonObj.transform.SetParent(dialog.transform, false);
        Button button = buttonObj.AddComponent<Button>();
        button.onClick.AddListener(() => {
            ReturnToHomePage();
            Destroy(dialog);
        });
        
        Image btnImage = buttonObj.AddComponent<Image>();
        btnImage.color = new Color(0.2f, 0.8f, 0.2f, 1f);
        
        // 按钮文本
        GameObject btnTextObj = new GameObject("ButtonText");
        btnTextObj.transform.SetParent(buttonObj.transform, false);
        Text btnText = btnTextObj.AddComponent<Text>();
        btnText.text = "返回主页";
        btnText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        btnText.fontSize = 18;
        btnText.color = Color.white;
        btnText.alignment = TextAnchor.MiddleCenter;
        
        RectTransform btnRect = buttonObj.GetComponent<RectTransform>();
        btnRect.anchoredPosition = new Vector2(0, -50);
        btnRect.sizeDelta = new Vector2(150, 40);
        
        RectTransform btnTextRect = btnTextObj.GetComponent<RectTransform>();
        btnTextRect.anchorMin = Vector2.zero;
        btnTextRect.anchorMax = Vector2.one;
        btnTextRect.sizeDelta = Vector2.zero;
        
        Debug.Log("[胜利UI] 创建了简单胜利对话框");
    }

    public void ReturnToHomePage()
    {
        Debug.Log("[胜利UI] 返回主页");
        SceneManager.LoadScene("Assets/Scenes/HomePage.unity");
    }
}