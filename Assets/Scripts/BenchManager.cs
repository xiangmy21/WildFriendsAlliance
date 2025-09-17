using UnityEngine;
using System.Collections.Generic; // 用于升星检查

public class BenchManager : MonoBehaviour
{
    // --- 单例 ---
    public static BenchManager Instance { get; private set; }

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            // (可选) DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // --- 核心字段 ---

    // 1. 把你的 "DraggableUnitIcon_Prefab" (可拖拽图标的预制体) 拖到这里
    public GameObject draggableUnitIconPrefab;

    // 2. 把你的 "Bench_Panel" (包含所有备战席槽位的父物体) 拖到这里
    public Transform benchSlotContainer;

    // 3. (进阶) 假设你的 UnitData 里有一个字段指向它的2星版本
    // public UnitData twoStarVersion; 

    private BenchSlot[] allBenchSlots;

    void Start()
    {
        // 自动获取所有子物体上的 BenchSlot 脚本
        allBenchSlots = benchSlotContainer.GetComponentsInChildren<BenchSlot>();
    }

    /// <summary>
    /// 尝试在备战席的第一个空位添加一个单位图标
    /// </summary>
    /// <param name="dataToAdd">要添加的单位数据</param>
    /// <returns>true 表示添加成功, false 表示备战席已满</returns>
    public bool AddUnitToBench(UnitData dataToAdd)
    {
        BenchSlot emptySlot = FindEmptySlot();

        if (emptySlot == null)
        {
            Debug.Log("备战席已满，无法添加！");
            return false;
        }

        // 在空槽位下实例化一个新的“可拖拽图标”
        GameObject iconGO = Instantiate(draggableUnitIconPrefab, emptySlot.transform);
        iconGO.transform.localPosition = Vector3.zero; // 确保它在槽位中心

        // 初始化这个图标 (你需要给 DraggableUnitIcon.cs 添加这个方法)
        DraggableUnitIcon iconScript = iconGO.GetComponent<DraggableUnitIcon>();
        iconScript.Initialize(dataToAdd);

        // -----------------------------------------------------
        // 进阶：检查升星 (根据你的策划案)
        // -----------------------------------------------------
        //CheckForStarUp(dataToAdd, 1); // 假设我们只检查1星升2星

        return true;
    }

    /// <summary>
    /// 查找第一个空的备战席槽位
    /// </summary>
    private BenchSlot FindEmptySlot()
    {
        foreach (BenchSlot slot in allBenchSlots)
        {
            // 如果一个槽位下面没有子物体 (没有图标)，说明它是空的
            if (slot.transform.childCount == 0)
            {
                return slot;
            }
        }
        return null; // 没找到空位
    }

    /// <summary>
    /// (进阶) 检查升星的简化逻辑
    /// </summary>
    //private void CheckForStarUp(UnitData unitData, int starLevel)
    //{
    //    // 1. 找到所有在备战席上“同名”且“同星级”的单位
    //    List<DraggableUnitIcon> matchingIcons = new List<DraggableUnitIcon>();
    //    foreach (BenchSlot slot in allBenchSlots)
    //    {
    //        if (slot.transform.childCount > 0)
    //        {
    //            DraggableUnitIcon icon = slot.GetComponentInChildren<DraggableUnitIcon>();
    //            // 假设 DraggableUnitIcon 有 unitData 和 starLevel 字段
    //            if (icon.unitData.unitName == unitData.unitName && icon.starLevel == starLevel)
    //            {
    //                matchingIcons.Add(icon);
    //            }
    //        }
    //    }

    //    // (TODO: 你还需要扫描“战场上”的同名单位)

    //    // 2. 如果找到了3个或更多
    //    if (matchingIcons.Count >= 3)
    //    {
    //        Debug.Log($"检测到3个 {unitData.unitName}，准备升星！");

    //        // 3. 销毁这3个旧的
    //        Destroy(matchingIcons[0].gameObject);
    //        Destroy(matchingIcons[1].gameObject);
    //        Destroy(matchingIcons[2].gameObject);

    //        // 4. 在备战席添加一个 2星 (或更高星) 的新单位
    //        //    (这要求你的 UnitData 里有关联 "twoStarVersion" 之类的字段)
    //        // if (unitData.twoStarVersion != null)
    //        // {
    //        //     AddUnitToBench(unitData.twoStarVersion);
    //        // }
    //    }
    //}
}