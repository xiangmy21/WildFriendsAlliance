using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class QuestionUI : MonoBehaviour
{
    [Header("UI组件")]
    public TextMeshProUGUI questionText;
    public TextMeshProUGUI animalNameText;
    public TextMeshProUGUI difficultyText;
    public Button[] optionButtons = new Button[4]; // A, B, C, D四个选项
    public TextMeshProUGUI[] optionTexts = new TextMeshProUGUI[4];

    [Header("结果显示")]
    public GameObject resultPanel;
    public TextMeshProUGUI resultText;
    public TextMeshProUGUI explanationText;
    public Button continueButton;

    private QuizCardData currentCard;
    private string selectedAnswer;

    void Start()
    {
        // 为每个选项按钮添加点击事件
        for (int i = 0; i < optionButtons.Length; i++)
        {
            int index = i; // 避免闭包问题
            if (optionButtons[i])
            {
                optionButtons[i].onClick.AddListener(() => OnOptionSelected(index));
            }
        }

        if (continueButton)
        {
            continueButton.onClick.AddListener(OnContinueClicked);
        }

        // 初始状态隐藏结果面板
        if (resultPanel) resultPanel.SetActive(false);
    }

    public void SetupQuestion(QuizCardData cardData)
    {
        currentCard = cardData;

        // 设置动物名称和难度
        if (animalNameText)
        {
            animalNameText.text = cardData.animalName;
        }

        if (difficultyText)
        {
            difficultyText.text = $"难度: {cardData.difficulty}";
        }

        // 设置问题文本
        if (questionText)
        {
            questionText.text = cardData.question.question;
        }

        // 加载按钮背景图片
        Sprite buttonSprite = null;
#if UNITY_EDITOR
        buttonSprite = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Images/UI/button1.png");
#else
        buttonSprite = Resources.Load<Sprite>("Images/UI/button1");
#endif

        // 设置选项文本和按钮样式
        for (int i = 0; i < optionTexts.Length && i < cardData.question.options.Length; i++)
        {
            if (optionTexts[i])
            {
                optionTexts[i].text = cardData.question.options[i];
                optionTexts[i].color = Color.black; // 设置文字颜色为黑色
            }

            // 设置按钮背景和尺寸
            if (optionButtons[i])
            {
                optionButtons[i].gameObject.SetActive(true);
                optionButtons[i].interactable = true;

                // 设置按钮背景图片
                Image buttonImage = optionButtons[i].GetComponent<Image>();
                if (buttonImage && buttonSprite)
                {
                    buttonImage.sprite = buttonSprite;
                    buttonImage.type = Image.Type.Sliced; // 使用切片模式
                }

                // 设置按钮尺寸
                RectTransform buttonRect = optionButtons[i].GetComponent<RectTransform>();
                if (buttonRect)
                {
                    // 禁用可能的布局组件
                    LayoutElement layoutElement = optionButtons[i].GetComponent<LayoutElement>();
                    if (layoutElement)
                    {
                        layoutElement.ignoreLayout = true;
                    }

                    // 强制设置尺寸
                    buttonRect.sizeDelta = new Vector2(180f, 42f); // 宽180（-10%），高42（+20%）

                    // 强制刷新布局
                    LayoutRebuilder.ForceRebuildLayoutImmediate(buttonRect);

                    // 设置按钮间距 - 紧凑排列，间隔5像素
                    Vector2 pos = buttonRect.anchoredPosition;
                    pos.y = 50f - (i * 47f); // 第一个按钮y=50，每个间隔47像素（42高度+5间距）
                    buttonRect.anchoredPosition = pos;
                }
            }
        }

        // 隐藏多余的按钮
        for (int i = cardData.question.options.Length; i < optionButtons.Length; i++)
        {
            if (optionButtons[i])
            {
                optionButtons[i].gameObject.SetActive(false);
            }
        }

        // 重置状态
        selectedAnswer = "";
        if (resultPanel) resultPanel.SetActive(false);
    }

    void OnOptionSelected(int optionIndex)
    {
        // 将索引转换为答案字母 (0->A, 1->B, 2->C, 3->D)
        selectedAnswer = char.ConvertFromUtf32(65 + optionIndex); // 65是'A'的ASCII码

        Debug.Log($"选择了选项: {selectedAnswer}");

        // 禁用所有选项按钮
        foreach (var button in optionButtons)
        {
            if (button) button.interactable = false;
        }

        // 提交答案
        if (AIRecruitmentManager.Instance)
        {
            AIRecruitmentManager.Instance.OnAnswerSubmitted(selectedAnswer);
        }

        // 显示结果
        ShowResult();
    }

    void ShowResult()
    {
        bool isCorrect = selectedAnswer == currentCard.question.correct_answer;

        if (resultPanel) resultPanel.SetActive(true);

        if (resultText)
        {
            resultText.text = isCorrect ? "回答正确！" : "回答错误！";
            resultText.color = isCorrect ? Color.green : Color.red;
        }

        if (explanationText)
        {
            explanationText.text = $"正确答案: {currentCard.question.correct_answer}\n\n解析: {currentCard.question.explanation}";
        }
    }

    void OnContinueClicked()
    {
        // 通知管理器关闭问答系统
        if (AIRecruitmentManager.Instance)
        {
            // AIRecruitmentManager会在ShowAnswerResult中自动关闭
        }
    }
}