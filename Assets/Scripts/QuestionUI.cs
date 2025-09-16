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

        // 设置选项文本
        for (int i = 0; i < optionTexts.Length && i < cardData.question.options.Length; i++)
        {
            if (optionTexts[i])
            {
                optionTexts[i].text = cardData.question.options[i];
            }

            // 激活对应的按钮
            if (optionButtons[i])
            {
                optionButtons[i].gameObject.SetActive(true);
                optionButtons[i].interactable = true;
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