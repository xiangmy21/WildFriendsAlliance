using System.Collections.Generic;
using System.IO;
using UnityEngine;
using System.Linq;

public class AIRecruitmentManager : MonoBehaviour
{
    public static AIRecruitmentManager Instance { get; private set; }

    [Header("UI引用")]
    public GameObject quizCardPanel; // 三选一卡片面板
    public GameObject questionPanel; // 问答界面面板
    public QuizCard[] quizCards = new QuizCard[3]; // 三张卡片
    public QuestionUI questionUI; // 问答UI组件

    [Header("动物友谊值数据")]
    public Dictionary<string, AnimalFriendshipData> animalFriendships = new Dictionary<string, AnimalFriendshipData>();

    [Header("问题数据")]
    private Dictionary<string, List<QuestionData>> animalQuestions = new Dictionary<string, List<QuestionData>>();

    [Header("当前状态")]
    private QuizCardData[] currentCards = new QuizCardData[3];
    private QuizCardData selectedCard;
    public bool isQuizActive = false;

    // 所有动物名称列表（对应湿地区域的棋子）
    private readonly string[] wetlandAnimals = {
        "青蛙", "蜻蜓", "白鹭", "鲤鱼", "水獭", "野鸭", "扬子鳄", "丹顶鹤", "中华鲟", "黑鹳"
    };

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeSystem();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void InitializeSystem()
    {
        LoadAllQuestions();
        InitializeFriendships();

        // 初始状态隐藏所有UI面板
        if (quizCardPanel) quizCardPanel.SetActive(false);
        if (questionPanel) questionPanel.SetActive(false);
    }

    void LoadAllQuestions()
    {
        foreach (string animalName in wetlandAnimals)
        {
            string filePath = Path.Combine(Application.dataPath, "..", "questions", animalName + ".json");

            if (File.Exists(filePath))
            {
                try
                {
                    string jsonContent = File.ReadAllText(filePath);
                    List<QuestionData> questions = SimpleJsonParser.ParseQuestionFile(jsonContent);
                    animalQuestions[animalName] = questions;
                    Debug.Log($"加载了 {animalName} 的 {questions.Count} 道题目");
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"加载 {animalName} 问题文件失败: {e.Message}");
                }
            }
            else
            {
                Debug.LogWarning($"找不到 {animalName} 的问题文件: {filePath}");
            }
        }
    }

    void InitializeFriendships()
    {
        foreach (string animalName in wetlandAnimals)
        {
            animalFriendships[animalName] = new AnimalFriendshipData(animalName);
        }
    }

    public void TriggerAIRecruitment()
    {
        if (isQuizActive) return;

        Debug.Log("AI智能招募官激活！");
        isQuizActive = true;

        GenerateThreeCards();
        ShowQuizCardPanel();
    }

    void GenerateThreeCards()
    {
        // 随机选择三个不同的动物
        var availableAnimals = wetlandAnimals.ToList();
        var selectedAnimals = new List<string>();

        for (int i = 0; i < 3; i++)
        {
            int randomIndex = Random.Range(0, availableAnimals.Count);
            string selectedAnimal = availableAnimals[randomIndex];
            selectedAnimals.Add(selectedAnimal);
            availableAnimals.RemoveAt(randomIndex);
        }

        // 为每个动物生成问题卡片
        for (int i = 0; i < 3; i++)
        {
            string animalName = selectedAnimals[i];
            QuestionData question = SelectQuestionForAnimal(animalName);
            currentCards[i] = new QuizCardData(animalName, question.difficulty, question);
        }
    }

    QuestionData SelectQuestionForAnimal(string animalName)
    {
        if (!animalQuestions.ContainsKey(animalName) || animalQuestions[animalName].Count == 0)
        {
            Debug.LogError($"没有找到 {animalName} 的问题数据");
            return null;
        }

        var questions = animalQuestions[animalName];
        int friendshipLevel = animalFriendships[animalName].friendshipLevel;

        // 根据友谊值加权选择题目难度（友谊值越高，越容易选到高难度题）
        var weightedQuestions = new List<QuestionData>();

        foreach (var question in questions)
        {
            // 基础权重
            int weight = 1;

            // 根据友谊值调整权重：友谊值每增加1，对应难度的权重增加
            if (question.difficulty <= friendshipLevel + 3)
            {
                weight += friendshipLevel;
            }

            // 添加到权重列表
            for (int i = 0; i < weight; i++)
            {
                weightedQuestions.Add(question);
            }
        }

        // 随机选择一个问题
        return weightedQuestions[Random.Range(0, weightedQuestions.Count)];
    }

    void ShowQuizCardPanel()
    {
        if (quizCardPanel)
        {
            quizCardPanel.SetActive(true);

            // 更新三张卡片的显示
            for (int i = 0; i < 3; i++)
            {
                if (quizCards[i] != null)
                {
                    quizCards[i].SetupCard(currentCards[i]);
                }
            }
        }
    }

    public void OnCardSelected(int cardIndex)
    {
        if (cardIndex < 0 || cardIndex >= currentCards.Length) return;

        selectedCard = currentCards[cardIndex];
        Debug.Log($"选择了卡片: {selectedCard.animalName} - 难度 {selectedCard.difficulty}");

        ShowQuestionPanel();
    }

    void ShowQuestionPanel()
    {
        if (quizCardPanel) quizCardPanel.SetActive(false);
        if (questionPanel) questionPanel.SetActive(true);

        if (questionUI != null)
        {
            questionUI.SetupQuestion(selectedCard);
        }
    }

    public void OnAnswerSubmitted(string selectedAnswer)
    {
        bool isCorrect = selectedAnswer == selectedCard.question.correct_answer;

        Debug.Log($"答案: {selectedAnswer}, 正确答案: {selectedCard.question.correct_answer}, 结果: {(isCorrect ? "正确" : "错误")}");

        ApplyAnswerEffects(selectedCard, isCorrect);

        // 显示结果并关闭问答界面
        ShowAnswerResult(isCorrect);
    }

    void ApplyAnswerEffects(QuizCardData card, bool isCorrect)
    {
        string animalName = card.animalName;
        var friendship = animalFriendships[animalName];

        if (isCorrect)
        {
            // 正确答案的奖励
            friendship.friendshipLevel += 1;
            friendship.battleBonus += 0.1f; // 本局游戏+10%净化能力

            Debug.Log($"{animalName} 友谊值增加到 {friendship.friendshipLevel}，获得 +10% 净化能力加成");

            // TODO: 实现卡牌强化和牌库加入逻辑
            // ApplyBattleBonus(animalName, 0.1f);
            // AddCardToPool(animalName, 1);
        }
        else
        {
            // 错误答案的惩罚
            friendship.battleBonus -= 0.05f; // 下一场战斗-5%生态韧性

            Debug.Log($"{animalName} 在下一场战斗中受到 -5% 生态韧性惩罚");

            // TODO: 实现牌库移除逻辑
            // RemoveCardFromPool(animalName, 1);
        }
    }

    void ShowAnswerResult(bool isCorrect)
    {
        // TODO: 显示答案结果UI
        Debug.Log($"答题结果: {(isCorrect ? "回答正确！" : "回答错误！")}");

        // 2秒后关闭问答界面
        Invoke("CloseQuizSystem", 2f);
    }

    void CloseQuizSystem()
    {
        if (questionPanel) questionPanel.SetActive(false);
        isQuizActive = false;

        Debug.Log("AI智能招募官关闭");

        // TODO: 继续游戏流程，返回到战斗准备阶段
    }

    // 获取动物友谊值信息的公共方法
    public AnimalFriendshipData GetAnimalFriendship(string animalName)
    {
        return animalFriendships.ContainsKey(animalName) ? animalFriendships[animalName] : null;
    }

    // 用于测试的方法
    [ContextMenu("测试触发AI招募官")]
    void TestTriggerRecruitment()
    {
        TriggerAIRecruitment();
    }
}