using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class QuizCard : MonoBehaviour
{
    [Header("UI组件")]
    public TextMeshProUGUI animalNameText;
    public TextMeshProUGUI difficultyText;
    public Button cardButton;
    public Image cardBackground;

    [Header("样式设置")]
    public Color[] difficultyColors = new Color[9]; // 难度1-9对应的颜色

    private QuizCardData cardData;
    private int cardIndex;

    void Start()
    {
        if (cardButton)
        {
            cardButton.onClick.AddListener(OnCardClicked);
        }
    }

    public void SetupCard(QuizCardData data)
    {
        cardData = data;

        // 设置动物名称
        if (animalNameText)
        {
            animalNameText.text = data.animalName;
        }

        // 设置难度显示
        if (difficultyText)
        {
            difficultyText.text = $"难度: {data.difficulty}";
        }

        // 设置背景颜色（根据难度）- 注释掉以保留背景图片
        // if (cardBackground && data.difficulty >= 1 && data.difficulty <= 9)
        // {
        //     cardBackground.color = difficultyColors[data.difficulty - 1];
        // }
    }

    public void SetCardIndex(int index)
    {
        cardIndex = index;
    }

    void OnCardClicked()
    {
        if (AIRecruitmentManager.Instance)
        {
            AIRecruitmentManager.Instance.OnCardSelected(cardIndex);
        }
    }
}