using UnityEngine;

public class TestCanvasManager : MonoBehaviour
{
    void Awake()
    {
        // 确保TestCanvas在场景切换时不被销毁
        DontDestroyOnLoad(gameObject);
        Debug.Log("[TestCanvas] 设置为DontDestroyOnLoad，确保UI在场景切换时保持存在");
    }
}