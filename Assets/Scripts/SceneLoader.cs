using UnityEngine;
using UnityEngine.SceneManagement;  // 引入场景管理命名空间

public class SceneLoader : MonoBehaviour
{
public void LoadLevelSelect()
    {
        //Debug.Log("跳过关卡选择，直接加载Level1场景");
        SceneManager.LoadScene("LevelSelect");  // 使用场景名称而非完整路径
    }

public void LoadLevel1()
    {
        Debug.Log("开始游戏 - 加载Level1场景");

        // 通知调试管理器游戏开始了
        if (DebugManager.Instance)
        {
            DebugManager.Instance.OnGameStarted();
        }

        SceneManager.LoadScene("Level1");
    }

public void StartGameWithAITest()
    {
        Debug.Log("开始游戏并触发AI招募官测试");

        // 直接加载Level1场景
        SceneManager.LoadScene("Level1");

        // 场景加载完成后会自动触发AI招募官（通过DebugManager）
    }
}
