using UnityEngine;
using System.Collections.Generic; // 我们需要使用 List (列表)

public class ShopManager : MonoBehaviour
{
    // --- 单例 ---
    public static ShopManager Instance { get; private set; }

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // --- 核心字段 (在 Inspector 中设置) ---

    [Header("Shop Settings")]
    public int refreshCost = 1; // 刷新花费

    [Header("References")]
    // 1. 把你的 "Shop_Panel" (包含所有 ShopSlot 的父物体) 拖到这里
    public Transform shopSlotContainer;

    // 2. 【关键】这是你的“总卡池”
    //    把你所有创建的 UnitData (动物 ScriptableObject) 资产都拖到这个列表里
    public List<UnitData> unitPool;

    private ShopSlot[] shopSlots; // 商店中所有的槽位

    void Start()
    {
        // 自动获取所有子物体上的 ShopSlot 脚本
        shopSlots = shopSlotContainer.GetComponentsInChildren<ShopSlot>();

        // 游戏开始时，进行第一次免费刷新
        RefreshShop(false);
    }

    /// <summary>
    /// 刷新商店的核心逻辑
    /// </summary>
    /// <param name="chargeGold">true = 收取金币刷新, false = 免费刷新 (例如波次结束时)</param>
    public void RefreshShop(bool chargeGold)
    {
        // 1. 如果是付费刷新，检查金币
        if (chargeGold)
        {
            if (GameManager.Instance.PlayerGold >= refreshCost)
            {
                GameManager.Instance.SpendGold(refreshCost);
            }
            else
            {
                Debug.Log("金币不足，无法刷新！");
                // TODO: 可以在UI上提示玩家
                return; // 钱不够，停止刷新
            }
        }

        // 2. 遍历所有商店槽位，并为它们分配一个随机单位
        foreach (ShopSlot slot in shopSlots)
        {
            UnitData randomUnit = GetRandomUnitFromPool();

            // ShopSlot.cs 脚本里必须有一个 DisplayUnit 方法
            // (我在上一个回答里已经帮你设计好了)
            slot.DisplayUnit(randomUnit);
        }
    }

    /// <summary>
    /// 从“总卡池”中随机抽取一个单位
    /// </summary>
    private UnitData GetRandomUnitFromPool()
    {
        // 如果卡池为空，返回 null (防止报错)
        if (unitPool == null || unitPool.Count == 0)
        {
            Debug.LogError("商店卡池 (Unit Pool) 是空的！");
            return null;
        }

        // 随机一个索引
        int randomIndex = Random.Range(0, unitPool.Count);

        // 返回该索引对应的 UnitData
        return unitPool[randomIndex];
    }

    /// <summary>
    /// 这是一个“公共”方法，专门给你的UI刷新按钮调用
    /// </summary>
    public void OnRefreshButtonPressed()
    {
        // 按钮刷新 = 付费刷新
        RefreshShop(true);
    }
}