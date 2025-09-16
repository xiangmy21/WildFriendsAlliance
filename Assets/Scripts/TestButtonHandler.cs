using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 测试按钮处理器 - 用于触发AI招募官系统测试
/// </summary>
public class TestButtonHandler : MonoBehaviour
{
    [Header("按钮引用")]
    public Button testButton;

    void Start()
    {
        // 自动查找按钮组件
        if (!testButton)
        {
            testButton = GetComponent<Button>();
        }

        // 绑定按钮点击事件
        if (testButton)
        {
            testButton.onClick.AddListener(OnTestButtonClicked);
            Debug.Log("测试按钮已绑定点击事件");
        }
        else
        {
            Debug.LogError("找不到Button组件！");
        }
    }

    void OnTestButtonClicked()
    {
        Debug.Log("测试按钮被点击 - 尝试触发AI招募官");

        // 方法1：通过AIRecruitmentManager直接触发
        if (AIRecruitmentManager.Instance)
        {
            Debug.Log("找到AIRecruitmentManager，正在触发招募官...");
            AIRecruitmentManager.Instance.TriggerAIRecruitment();
        }
        else
        {
            Debug.LogError("没有找到AIRecruitmentManager实例！");
        }

        // 方法2：通过DebugManager触发
        if (DebugManager.Instance)
        {
            Debug.Log("通过DebugManager触发招募官...");
            DebugManager.Instance.OnGameStarted();
        }

        // 方法3：通过SimpleUITester触发
        SimpleUITester uiTester = FindObjectOfType<SimpleUITester>();
        if (uiTester)
        {
            Debug.Log("找到SimpleUITester，调用测试方法...");
            uiTester.TestAIRecruitment();
        }
    }

    // 公共方法，可以在Inspector中直接调用
    [ContextMenu("测试AI招募官")]
    public void TestAIRecruitmentFromMenu()
    {
        OnTestButtonClicked();
    }
}