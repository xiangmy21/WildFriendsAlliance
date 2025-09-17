using System.Collections.Generic;
using System.IO;
using UnityEngine;
using System.Linq;
using UnityEngine.Networking;
using System.Collections;

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

    [Header("牌库系统")]
    // 牌库数据结构 - 记录每种动物卡牌的数量
    public Dictionary<string, int> animalDeck = new Dictionary<string, int>();

    [Header("问题数据")]
    private Dictionary<string, List<QuestionData>> animalQuestions = new Dictionary<string, List<QuestionData>>();

    [Header("当前状态")]
    private QuizCardData[] currentCards = new QuizCardData[3];
    private QuizCardData selectedCard;
    

    [Header("UI交互控制")]
    private Canvas[] gameCanvases; // 存储游戏中UI Canvas的引用
public bool isQuizActive = false;

    

    // 动物名称到卡片文件名的映射
    private readonly Dictionary<string, string> animalCardMapping = new Dictionary<string, string>()
    {
        {"松鼠", "squirrel_副本"},
        {"刺猬", "ciwei_副本"},
        {"野猪", "pig_副本"},
        {"猫头鹰", "owl_副本"},
        {"赤狐", "fox_副本"},
        {"小熊猫", "xiaoxiongmao_副本"},
        {"梅花鹿", "deer_副本"},
        {"金丝猴", "monkey_副本"},
        {"大熊猫", "panda_副本"},
        {"东北虎", "tiger_副本"}
    };
// 所有动物名称列表（对应湿地区域的棋子）
    private readonly string[] Forest = {
        "松鼠", "刺猬", "野猪", "猫头鹰", "赤狐",
        "小熊猫", "梅花鹿", "金丝猴", "大熊猫", "东北虎"
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
#if UNITY_WEBGL && !UNITY_EDITOR
        StartCoroutine(LoadAllQuestionsCoroutine());
#else
        LoadAllQuestions();
#endif
        InitializeFriendships();
        InitializeDeck();

        // 立即查找并保护TestCanvas
        GameObject testCanvas = GameObject.Find("TestCanvas");
        if (testCanvas != null)
        {
            DontDestroyOnLoad(testCanvas);
            Debug.Log("[初始化] TestCanvas已设置为DontDestroyOnLoad");
        }
        else
        {
            Debug.LogWarning("[初始化] 未找到TestCanvas，无法设置DontDestroyOnLoad");
        }

        // 确保UI面板使用DontDestroyOnLoad
        if (quizCardPanel != null)
        {
            DontDestroyOnLoad(quizCardPanel);
            Debug.Log("[初始化] Quiz面板设置为DontDestroyOnLoad");
        }

        if (questionPanel != null)
        {
            DontDestroyOnLoad(questionPanel);
            Debug.Log("[初始化] Question面板设置为DontDestroyOnLoad");
        }

        // 初始状态隐藏所有UI面板
        if (quizCardPanel) quizCardPanel.SetActive(false);
        if (questionPanel) questionPanel.SetActive(false);
    }

// 动态查找UI面板
// 动态查找UI面板
// 动态查找UI面板
// 动态查找UI面板
    void FindUIReferences()
    {
        Debug.Log($"[调试] 当前UI状态 - quizCardPanel: {(quizCardPanel != null ? quizCardPanel.name : "null")}, questionPanel: {(questionPanel != null ? questionPanel.name : "null")}");
        
        // 检查当前UI引用是否有效（是否被销毁）
        bool needFind = false;
        if (quizCardPanel == null)
        {
            Debug.Log("[调试] quizCardPanel为null，需要查找");
            needFind = true;
        }
        else if (quizCardPanel.scene.name == null || quizCardPanel.scene.name == "")
        {
            Debug.Log("[调试] quizCardPanel已被销毁，需要重新查找");
            quizCardPanel = null;
            needFind = true;
        }
        
        if (questionPanel == null)
        {
            Debug.Log("[调试] questionPanel为null，需要查找");
            needFind = true;
        }
        else if (questionPanel.scene.name == null || questionPanel.scene.name == "")
        {
            Debug.Log("[调试] questionPanel已被销毁，需要重新查找");
            questionPanel = null;
            needFind = true;
        }
        
        if (needFind)
        {
            Debug.Log("[调试] 开始重新查找UI引用");
            
            // 方法1：查找 TestCanvas 下的面板
            GameObject testCanvas = GameObject.Find("TestCanvas");
            if (testCanvas != null)
            {
                Debug.Log("[成功] 找到TestCanvas");
                
                // 确保TestCanvas使用DontDestroyOnLoad
                if (testCanvas.scene.name != "DontDestroyOnLoad")
                {
                    DontDestroyOnLoad(testCanvas);
                    Debug.Log("[成功] TestCanvas已设置为DontDestroyOnLoad");
                }
                
                // 列出TestCanvas下的所有子对象
                Debug.Log($"[调试] TestCanvas下有{testCanvas.transform.childCount}个子对象:");
                for (int i = 0; i < testCanvas.transform.childCount; i++)
                {
                    Transform child = testCanvas.transform.GetChild(i);
                    Debug.Log($"[调试] TestCanvas子对象[{i}]: {child.name}");
                }
                
                // 尝试多种可能的名称查找Quiz Panel
                Transform quizTransform = testCanvas.transform.Find("Quiz Panel");
                if (quizTransform == null) quizTransform = testCanvas.transform.Find("QuizPanel");
                if (quizTransform == null) quizTransform = testCanvas.transform.Find("Quiz");
                if (quizTransform == null) quizTransform = testCanvas.transform.Find("quiz");
                
                if (quizTransform != null)
                {
                    quizCardPanel = quizTransform.gameObject;
                    Debug.Log($"[成功] 找到Quiz Panel: {quizTransform.name}");
                }
                else
                {
                    Debug.LogWarning("[警告] 在TestCanvas中未找到Quiz Panel");
                }
                
                // 尝试多种可能的名称查找Question Panel
                Transform questionTransform = testCanvas.transform.Find("Question Panel");
                if (questionTransform == null) questionTransform = testCanvas.transform.Find("QuestionPanel");
                if (questionTransform == null) questionTransform = testCanvas.transform.Find("Question");
                if (questionTransform == null) questionTransform = testCanvas.transform.Find("question");
                
                if (questionTransform != null)
                {
                    questionPanel = questionTransform.gameObject;
                    Debug.Log($"[成功] 找到Question Panel: {questionTransform.name}");
                }
                else
                {
                    Debug.LogWarning("[警告] 在TestCanvas中未找到Question Panel");
                }
            }
            else
            {
                Debug.LogWarning("[警告] 未找到TestCanvas，尝试全局搜索");
                
                // 方法2：全局搜索包含Quiz或Question的GameObject
                GameObject[] allObjects = Resources.FindObjectsOfTypeAll<GameObject>();
                Debug.Log($"[调试] 全局搜索中，共找到{allObjects.Length}个对象");
                
                int quizCount = 0, questionCount = 0;
                foreach (GameObject obj in allObjects)
                {
                    if (obj.name.ToLower().Contains("quiz"))
                    {
                        quizCount++;
                        Debug.Log($"[调试] 找到包含quiz的对象: {obj.name}");
                        if (quizCardPanel == null)
                        {
                            quizCardPanel = obj;
                            Debug.Log($"[成功] 通过全局搜索找到Quiz Panel: {obj.name}");
                        }
                    }
                    if (obj.name.ToLower().Contains("question"))
                    {
                        questionCount++;
                        Debug.Log($"[调试] 找到包含question的对象: {obj.name}");
                        if (questionPanel == null)
                        {
                            questionPanel = obj;
                            Debug.Log($"[成功] 通过全局搜索找到Question Panel: {obj.name}");
                        }
                    }
                }
                Debug.Log($"[调试] 全局搜索结果 - quiz对象:{quizCount}个, question对象:{questionCount}个");
            }
        }
        else
        {
            Debug.Log("[调试] UI引用有效，跳过查找");
        }
    }


    // WebGL兼容的异步文件加载
    IEnumerator LoadAllQuestionsCoroutine()
    {
        foreach (string animalName in Forest)
        {
            yield return StartCoroutine(LoadAnimalQuestionsCoroutine(animalName));
        }
    }

    IEnumerator LoadAnimalQuestionsCoroutine(string animalName)
    {
        string filePath;

        filePath = Path.Combine(Application.streamingAssetsPath, "questions", animalName + ".json");

        UnityWebRequest request = UnityWebRequest.Get(filePath);
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            try
            {
                string jsonContent = request.downloadHandler.text;
                List<QuestionData> questions = SimpleJsonParser.ParseQuestionFile(jsonContent);
                animalQuestions[animalName] = questions;
                Debug.Log($"加载了 {animalName} 的 {questions.Count} 道题目");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"解析 {animalName} 问题文件失败: {e.Message}");
            }
        }
        else
        {
            Debug.LogError($"加载 {animalName} 问题文件失败: {request.error}");
            Debug.LogWarning($"文件路径: {filePath}");
        }

        request.Dispose();
    }

    void LoadAllQuestions()
    {
        foreach (string animalName in Forest)
        {
            string filePath = Path.Combine(Application.streamingAssetsPath, "questions", animalName + ".json");

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
        foreach (string animalName in Forest)
        {
            animalFriendships[animalName] = new AnimalFriendshipData(animalName);
        }
    }

public void TriggerAIRecruitment()
    {
        if (isQuizActive) return;

        Debug.Log("AI智能招募官激活！");
        isQuizActive = true;

        // 在触发前动态查找UI引用
        FindUIReferences();

        // 禁用其他UI交互
        DisableOtherUIInteractions();

        GenerateThreeCards();
        ShowQuizCardPanel();
    }

    void GenerateThreeCards()
    {
        // 随机选择三个不同的动物
        var availableAnimals = Forest.ToList();
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
        Debug.Log("[调试] ShowQuizCardPanel被调用");
        
        if (quizCardPanel)
        {
            quizCardPanel.SetActive(true);
            Debug.Log("[成功] 显示问答卡片面板");

            // 更新三张卡片的显示
            for (int i = 0; i < 3; i++)
            {
                if (quizCards[i] != null)
                {
                    quizCards[i].SetupCard(currentCards[i]);
                    Debug.Log($"[成功] 设置卡片 {i}: {currentCards[i].animalName}");
                }
                else
                {
                    Debug.LogWarning($"[错误] quizCards[{i}] 为 null");
                }
            }
        }
        else
        {
            Debug.LogError("[错误] quizCardPanel 为 null，无法显示问答界面");
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

        // 恢复其他UI交互
        EnableOtherUIInteractions();

        Debug.Log("AI智能招募官关闭");

        // 通知GameManager问答完成，继续游戏流程
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnQuizCompleted();
        }
        else
        {
            Debug.LogWarning("GameManager未找到，无法通知问答完成");
        }
    }

    // 获取动物友谊值信息的公共方法
    public AnimalFriendshipData GetAnimalFriendship(string animalName)
    {
        return animalFriendships.ContainsKey(animalName) ? animalFriendships[animalName] : null;
    }

    
    // UI交互控制方法
void DisableOtherUIInteractions()
    {
        // 获取所有游戏Canvas（除了问答相关的）
        Canvas[] allCanvases = FindObjectsOfType<Canvas>();
        gameCanvases = allCanvases;
        
        foreach (Canvas canvas in allCanvases)
        {
            // 保留问答相关的Canvas可交互
            if (IsQuizRelatedCanvas(canvas))
                continue;
                
            // 通过CanvasGroup控制交互，如果没有就添加一个
            CanvasGroup canvasGroup = canvas.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = canvas.gameObject.AddComponent<CanvasGroup>();
            }
            
            canvasGroup.interactable = false;
            Debug.Log($"禁用Canvas: {canvas.name}");
        }
    }
    
void EnableOtherUIInteractions()
    {
        if (gameCanvases != null)
        {
            foreach (Canvas canvas in gameCanvases)
            {
                if (canvas != null)
                {
                    // 通过CanvasGroup恢复交互
                    CanvasGroup canvasGroup = canvas.GetComponent<CanvasGroup>();
                    if (canvasGroup != null)
                    {
                        canvasGroup.interactable = true;
                        Debug.Log($"恢复Canvas: {canvas.name}");
                    }
                }
            }
        }
    }
    
    bool IsQuizRelatedCanvas(Canvas canvas)
    {
        // 检查Canvas是否与问答系统相关
        if (canvas == null) return false;
        
        // 检查是否包含问答相关的UI组件
        if (quizCardPanel != null && canvas.transform.IsChildOf(quizCardPanel.transform)) return true;
        if (questionPanel != null && canvas.transform.IsChildOf(questionPanel.transform)) return true;
        if (quizCardPanel != null && quizCardPanel.transform.IsChildOf(canvas.transform)) return true;
        if (questionPanel != null && questionPanel.transform.IsChildOf(canvas.transform)) return true;
        
        return false;
    }

// 加载动物对应的卡片背景
    public Sprite LoadAnimalCardSprite(string animalName)
    {
        if (animalCardMapping.ContainsKey(animalName))
        {
            string cardFileName = animalCardMapping[animalName];
            string cardPath = $"Assets/card/{cardFileName}.png";
            
            Debug.Log($"[卡片加载] 尝试加载 {animalName} 的卡片: {cardPath}");
            
            // 尝试通过Resources加载
            Sprite cardSprite = Resources.Load<Sprite>($"card/{cardFileName}");
            if (cardSprite != null)
            {
                Debug.Log($"[成功] 通过Resources加载卡片: {cardFileName}");
                return cardSprite;
            }
            
#if UNITY_EDITOR
            // 在编辑器中使用AssetDatabase加载
            cardSprite = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>(cardPath);
            if (cardSprite != null)
            {
                Debug.Log($"[成功] 通过AssetDatabase加载卡片: {cardFileName}");
                return cardSprite;
            }
#endif
            
            Debug.LogWarning($"[失败] 无法加载 {animalName} 的卡片: {cardPath}");
        }
        else
        {
            Debug.LogWarning($"[警告] 未找到 {animalName} 的卡片映射");
        }
        
        return null;
    }

    // =========================== 牌库管理系统 ===========================

    // 初始化牌库
    void InitializeDeck()
    {
        foreach (string animalName in Forest)
        {
            animalDeck[animalName] = 0; // 初始数量为0
        }
        Debug.Log("[牌库系统] 牌库初始化完成，包含10种动物");
    }

    // 添加卡牌到牌库
    public void AddCardToPool(string animalName, int count = 1)
    {
        if (animalDeck.ContainsKey(animalName))
        {
            animalDeck[animalName] += count;
            Debug.Log($"[牌库系统] 牌库更新: {animalName} x{animalDeck[animalName]}");

            // 通知UI更新
            if (GameUIController.Instance != null)
            {
                GameUIController.Instance.UpdateDeckInfo();
            }
        }
        else
        {
            Debug.LogWarning($"[牌库系统] 无效的动物名称: {animalName}");
        }
    }

    // 从牌库移除卡牌
    public void RemoveCardFromPool(string animalName, int count = 1)
    {
        if (animalDeck.ContainsKey(animalName))
        {
            animalDeck[animalName] = Mathf.Max(0, animalDeck[animalName] - count);
            Debug.Log($"[牌库系统] 牌库更新: {animalName} x{animalDeck[animalName]}");

            // 通知UI更新
            if (GameUIController.Instance != null)
            {
                GameUIController.Instance.UpdateDeckInfo();
            }
        }
        else
        {
            Debug.LogWarning($"[牌库系统] 无效的动物名称: {animalName}");
        }
    }

    // 获取牌库中指定动物的数量
    public int GetCardCount(string animalName)
    {
        if (animalDeck.ContainsKey(animalName))
        {
            return animalDeck[animalName];
        }
        return 0;
    }

    // 获取整个牌库的副本（只读）
    public Dictionary<string, int> GetDeckCopy()
    {
        return new Dictionary<string, int>(animalDeck);
    }

    // 答题正确时调用，随机获得一张卡牌
    public void OnAnswerCorrect()
    {
        if (Forest.Length > 0)
        {
            string randomAnimal = Forest[Random.Range(0, Forest.Length)];
            AddCardToPool(randomAnimal, 1);
            Debug.Log($"[牌库系统] 答题正确！获得 {randomAnimal} 卡牌 x1");
        }
    }

    // 答题错误时调用，随机失去一张卡牌
    public void OnAnswerWrong()
    {
        // 找到所有有卡牌的动物
        List<string> availableAnimals = new List<string>();
        foreach (var pair in animalDeck)
        {
            if (pair.Value > 0)
            {
                availableAnimals.Add(pair.Key);
            }
        }

        if (availableAnimals.Count > 0)
        {
            string randomAnimal = availableAnimals[Random.Range(0, availableAnimals.Count)];
            RemoveCardFromPool(randomAnimal, 1);
            Debug.Log($"[牌库系统] 答题错误！失去 {randomAnimal} 卡牌 x1");
        }
        else
        {
            Debug.Log("[牌库系统] 答题错误但牌库为空，无卡牌可失去");
        }
    }

// 用于测试的方法
    [ContextMenu("测试触发AI招募官")]
    void TestTriggerRecruitment()
    {
        TriggerAIRecruitment();
    }

    [ContextMenu("测试添加卡牌")]
    void TestAddCards()
    {
        AddCardToPool("松鼠", 2);
        AddCardToPool("赤狐", 1);
        AddCardToPool("东北虎", 1);
    }
}