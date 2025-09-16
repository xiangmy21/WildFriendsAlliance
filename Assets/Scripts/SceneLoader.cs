using UnityEngine;
using UnityEngine.SceneManagement;  // 引入场景管理命名空间

public class SceneLoader : MonoBehaviour
{
    public void LoadLevelSelect()
    {
        SceneManager.LoadScene("LevelSelect");  // 用场景名字
        // 或者用 build index： SceneManager.LoadScene(1);
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
