using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 简化版UI测试组件 - 用于快速创建和测试AI招募官系统
/// 当没有完整UI时使用这个组件来模拟界面
/// </summary>
public class SimpleUITester : MonoBehaviour
{
    [Header("是否使用简化UI")]
    public bool useSimpleUI = true;

    [Header("Canvas引用")]
    public Canvas mainCanvas;

    [Header("卡片背景图片")]
    public Sprite cardBackground;

    
[Header("字体设置")]
    public TMPro.TMP_FontAsset chineseFont;

    [Header("创建的UI元素")]
    private GameObject quizCardPanel;
    private GameObject questionPanel;
    private QuizCard[] quizCards = new QuizCard[3];
    private QuestionUI questionUI;

    void Start()
    {
        // 加载中文字体
        if (chineseFont == null)
        {
            chineseFont = UnityEngine.Resources.Load<TMPro.TMP_FontAsset>("Fonts/ZIHUN144HAO-LANGYUANTI-2 SDF");
            if (chineseFont == null)
            {
                // 尝试通过AssetDatabase加载
                #if UNITY_EDITOR
                chineseFont = UnityEditor.AssetDatabase.LoadAssetAtPath<TMPro.TMP_FontAsset>("Assets/TextMesh Pro/Fonts/ZIHUN144HAO-LANGYUANTI-2 SDF.asset");
                #endif
            }
            if (chineseFont == null)
            {
                Debug.LogWarning("无法加载中文字体，使用默认字体");
            }
        }

        // 加载卡片背景图片
        if (cardBackground == null)
        {
            Debug.LogWarning("cardBackground在Inspector中为空，尝试通过代码加载");
            #if UNITY_EDITOR
            cardBackground = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Images/UI/questionbackground.png");
            if (cardBackground != null)
            {
                Debug.Log("成功通过AssetDatabase加载卡片背景");
            }
            else
            {
                Debug.LogError("无法加载卡片背景图片");
            }
            #endif
        }
        else
        {
            Debug.Log($"cardBackground已设置: {cardBackground.name}");
        }

        if (useSimpleUI)
        {
            CreateSimpleUI();
            SetupAIRecruitmentManager();
        }

        // 找到测试按钮并绑定事件
        SetupTestButton();
    }

    // 辅助方法：创建文本组件并设置字体
    TextMeshProUGUI CreateTextComponent(GameObject parent, string text, int fontSize, TextAlignmentOptions alignment, Color color)
    {
        var textComponent = parent.AddComponent<TextMeshProUGUI>();
        textComponent.text = text;
        textComponent.fontSize = fontSize;
        textComponent.alignment = alignment;
        textComponent.color = color;

        if (chineseFont != null)
        {
            textComponent.font = chineseFont;
        }

        return textComponent;
    }

    void SetupTestButton()
    {
        // 查找测试按钮
        GameObject testButtonObj = GameObject.Find("TestButton");
        if (testButtonObj)
        {
            UnityEngine.UI.Button testButton = testButtonObj.GetComponent<UnityEngine.UI.Button>();
            if (testButton)
            {
                // 清除现有的onClick事件
                testButton.onClick.RemoveAllListeners();

                // 添加新的onClick事件
                testButton.onClick.AddListener(() => {
                    Debug.Log("测试按钮被点击！触发AI招募官...");
                    TestAIRecruitment();
                });

                Debug.Log("测试按钮事件已绑定");
            }
        }
    }

    void CreateSimpleUI()
    {
        if (!mainCanvas)
        {
            // 创建Canvas
            GameObject canvasGO = new GameObject("TestCanvas");
            mainCanvas = canvasGO.AddComponent<Canvas>();
            mainCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasGO.AddComponent<CanvasScaler>();
            canvasGO.AddComponent<GraphicRaycaster>();
        }

        CreateQuizCardPanel();
        CreateQuestionPanel();
    }

    void CreateQuizCardPanel()
    {
        // 创建三选一卡片面板
        quizCardPanel = new GameObject("QuizCardPanel");
        quizCardPanel.transform.SetParent(mainCanvas.transform, false);

        var panelImage = quizCardPanel.AddComponent<Image>();
        panelImage.color = new Color(0, 0, 0, 0.8f); // 半透明黑色背景

        var rectTransform = quizCardPanel.GetComponent<RectTransform>();
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.one;
        rectTransform.sizeDelta = Vector2.zero;

        // 创建标题
        GameObject title = new GameObject("Title");
        title.transform.SetParent(quizCardPanel.transform, false);
        var titleText = CreateTextComponent(title, "AI智能招募官 - 选择一个问题", 24, TextAlignmentOptions.Center, Color.white);

        var titleRect = title.GetComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0, 0.8f);
        titleRect.anchorMax = new Vector2(1, 1);
        titleRect.sizeDelta = Vector2.zero;

        // 创建三张卡片
        for (int i = 0; i < 3; i++)
        {
            GameObject card = CreateSimpleCard(i, quizCardPanel.transform);
            quizCards[i] = card.GetComponent<QuizCard>();
        }

        quizCardPanel.SetActive(false);
    }

    GameObject CreateSimpleCard(int index, Transform parent)
    {
        // --- 1. 创建外部容器 (CardContainer) ---
        // 这个容器负责定位和定义一个固定的显示区域
        GameObject cardContainer = new GameObject($"CardContainer{index}");
        cardContainer.transform.SetParent(parent, false);
        var containerRect = cardContainer.AddComponent<RectTransform>();

        // 设置容器的位置和大小
        float cardWidth = 200f;
        float cardHeight = 150f; // 这是容器的大小，不是图片的大小
        float spacing = 50f;
        float totalWidth = (cardWidth * 3) + (spacing * 2);
        float startX = -totalWidth / 2 + cardWidth / 2;

        containerRect.anchoredPosition = new Vector2(startX + (cardWidth + spacing) * index, -50);
        containerRect.sizeDelta = new Vector2(cardWidth, cardHeight);

        // --- 2. 创建内部图片 (CardImage) ---
        // 这个子物体负责显示图片并保持其宽高比
        GameObject cardImageGO = new GameObject("CardImage");
        cardImageGO.transform.SetParent(containerRect.transform, false);
        var imageRect = cardImageGO.AddComponent<RectTransform>();

        // 让图片撑满父容器
        imageRect.anchorMin = Vector2.zero;
        imageRect.anchorMax = Vector2.one;
        imageRect.sizeDelta = Vector2.zero;

        var cardImage = cardImageGO.AddComponent<Image>();
        
        // --- 3. 使用CardImageController替代AspectRatioFitter ---
        var cardController = cardImageGO.AddComponent<CardImageController>();
        
        // 设置图片显示参数
        cardController.minWidth = 180f;
        cardController.minHeight = 120f;
        cardController.maxWidth = 220f;
        cardController.maxHeight = 160f;
        cardController.aspectMode = CardImageController.AspectMode.EnvelopeParent;

        // --- 4. 设置图片 ---
        if (cardBackground != null)
        {
            cardController.SetImage(cardBackground);
            Debug.Log($"已设置卡片{index}背景图片: {cardBackground.name}，宽高比: {cardController.AspectRatio:F2}");
        }
        else
        {
            cardImage.color = Color.gray; // 使用灰色背景以示区分
            Debug.LogWarning($"卡片{index}背景图片为空");
        }

        // --- 4. 将组件和逻辑绑定到容器上 ---
        // 按钮和业务逻辑脚本应该在最外层的容器上，以便整个区域都能被点击
        var button = cardContainer.AddComponent<Button>();
        // **重要**：Button 需要一个 Image 来接收点击事件，我们让容器的 Image 透明
        var containerImage = cardContainer.AddComponent<Image>();
        containerImage.color = Color.clear;
        button.targetGraphic = containerImage;

        var quizCard = cardContainer.AddComponent<QuizCard>();
        quizCard.cardButton = button;
        quizCard.cardBackground = cardImage; // 背景引用指向内部的 Image
        quizCard.SetCardIndex(index);

        // --- 5. 创建文本并设置父物体 ---
        // 文本的父物体是容器，这样它们会显示在图片之上

        // 动物名称文本
        GameObject animalName = new GameObject("AnimalName");
        animalName.transform.SetParent(cardContainer.transform, false); // 父物体是容器
        var animalText = CreateTextComponent(animalName, "动物", 18, TextAlignmentOptions.Center, Color.white);
        quizCard.animalNameText = animalText;

        var animalRect = animalName.GetComponent<RectTransform>();
        animalRect.anchorMin = new Vector2(0, 0.5f);
        animalRect.anchorMax = new Vector2(1, 1);
        animalRect.sizeDelta = Vector2.zero;

        // 难度文本
        GameObject difficulty = new GameObject("Difficulty");
        difficulty.transform.SetParent(cardContainer.transform, false); // 父物体是容器
        var diffText = CreateTextComponent(difficulty, "难度: 1", 14, TextAlignmentOptions.Center, Color.white);
        quizCard.difficultyText = diffText;

        var diffRect = difficulty.GetComponent<RectTransform>();
        diffRect.anchorMin = new Vector2(0, 0);
        diffRect.anchorMax = new Vector2(1, 0.5f);
        diffRect.sizeDelta = Vector2.zero;

        // 返回最外层的容器
        return cardContainer;
    }

    void CreateQuestionPanel()
    {
        // 创建问答面板
        questionPanel = new GameObject("QuestionPanel");
        questionPanel.transform.SetParent(mainCanvas.transform, false);

        var panelImage = questionPanel.AddComponent<Image>();
        panelImage.color = new Color(0.1f, 0.1f, 0.1f, 0.9f); // 深灰色背景

        var rectTransform = questionPanel.GetComponent<RectTransform>();
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.one;
        rectTransform.sizeDelta = Vector2.zero;

        // 添加QuestionUI组件
        questionUI = questionPanel.AddComponent<QuestionUI>();

        // 创建问题文本
        GameObject questionText = new GameObject("QuestionText");
        questionText.transform.SetParent(questionPanel.transform, false);
        var qText = CreateTextComponent(questionText, "问题内容", 20, TextAlignmentOptions.Center, Color.white);
        questionUI.questionText = qText;

        var qRect = questionText.GetComponent<RectTransform>();
        qRect.anchorMin = new Vector2(0.1f, 0.7f);
        qRect.anchorMax = new Vector2(0.9f, 0.9f);
        qRect.sizeDelta = Vector2.zero;

        // 创建选项按钮
        questionUI.optionButtons = new Button[4];
        questionUI.optionTexts = new TextMeshProUGUI[4];

        for (int i = 0; i < 4; i++)
        {
            GameObject option = CreateOptionButton(i, questionPanel.transform);
            questionUI.optionButtons[i] = option.GetComponent<Button>();
            questionUI.optionTexts[i] = option.GetComponentInChildren<TextMeshProUGUI>();
        }

        // 创建结果面板
        CreateResultPanel(questionPanel.transform);

        questionPanel.SetActive(false);
    }

    GameObject CreateOptionButton(int index, Transform parent)
    {
        GameObject option = new GameObject($"Option{index}");
        option.transform.SetParent(parent, false);

        var optionImage = option.AddComponent<Image>();
        optionImage.color = Color.white;

        var button = option.AddComponent<Button>();

        // 选项文本
        GameObject optionText = new GameObject("Text");
        optionText.transform.SetParent(option.transform, false);
        var text = CreateTextComponent(optionText, $"选项 {char.ConvertFromUtf32(65 + index)}", 16, TextAlignmentOptions.Center, Color.black);

        var textRect = optionText.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.sizeDelta = Vector2.zero;

        // 设置选项位置
        var optionRect = option.GetComponent<RectTransform>();
        float buttonHeight = 50f;
        float spacing = 10f;
        float startY = 0.6f - (index * (buttonHeight + spacing)) / 1000f;

        optionRect.anchorMin = new Vector2(0.1f, startY - buttonHeight / 1000f);
        optionRect.anchorMax = new Vector2(0.9f, startY);
        optionRect.sizeDelta = Vector2.zero;

        return option;
    }

    void CreateResultPanel(Transform parent)
    {
        GameObject resultPanel = new GameObject("ResultPanel");
        resultPanel.transform.SetParent(parent, false);

        var resultImage = resultPanel.AddComponent<Image>();
        resultImage.color = new Color(0, 0, 0, 0.8f);

        var resultRect = resultPanel.GetComponent<RectTransform>();
        resultRect.anchorMin = new Vector2(0.2f, 0.2f);
        resultRect.anchorMax = new Vector2(0.8f, 0.8f);
        resultRect.sizeDelta = Vector2.zero;

        // 结果文本
        GameObject resultText = new GameObject("ResultText");
        resultText.transform.SetParent(resultPanel.transform, false);
        var rText = CreateTextComponent(resultText, "结果", 24, TextAlignmentOptions.Center, Color.white);

        var rTextRect = resultText.GetComponent<RectTransform>();
        rTextRect.anchorMin = new Vector2(0, 0.7f);
        rTextRect.anchorMax = new Vector2(1, 1);
        rTextRect.sizeDelta = Vector2.zero;

        // 解析文本
        GameObject explanationText = new GameObject("ExplanationText");
        explanationText.transform.SetParent(resultPanel.transform, false);
        var eText = CreateTextComponent(explanationText, "解析内容", 16, TextAlignmentOptions.TopLeft, Color.white);

        var eTextRect = explanationText.GetComponent<RectTransform>();
        eTextRect.anchorMin = new Vector2(0.1f, 0.1f);
        eTextRect.anchorMax = new Vector2(0.9f, 0.7f);
        eTextRect.sizeDelta = Vector2.zero;

        questionUI.resultPanel = resultPanel;
        questionUI.resultText = rText;
        questionUI.explanationText = eText;

        resultPanel.SetActive(false);
    }

    void SetupAIRecruitmentManager()
    {
        // 查找或创建AIRecruitmentManager
        if (AIRecruitmentManager.Instance == null)
        {
            GameObject aiManager = new GameObject("AIRecruitmentManager");
            aiManager.AddComponent<AIRecruitmentManager>();
        }

        // 设置UI引用
        if (AIRecruitmentManager.Instance)
        {
            AIRecruitmentManager.Instance.quizCardPanel = quizCardPanel;
            AIRecruitmentManager.Instance.questionPanel = questionPanel;
            AIRecruitmentManager.Instance.quizCards = quizCards;
            AIRecruitmentManager.Instance.questionUI = questionUI;

            Debug.Log("已设置AI招募官UI引用");
        }
    }

    [ContextMenu("测试AI招募官")]
    public void TestAIRecruitment()
    {
        if (AIRecruitmentManager.Instance)
        {
            AIRecruitmentManager.Instance.TriggerAIRecruitment();
        }
    }
}