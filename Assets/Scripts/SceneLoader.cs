using UnityEngine;
using UnityEngine.SceneManagement;  // 引入场景管理命名空间

public class SceneLoader : MonoBehaviour
{
    public void LoadLevelSelect()
    {
        SceneManager.LoadScene("LevelSelect");  // 用场景名字
        // 或者用 build index： SceneManager.LoadScene(1);
    }
}
