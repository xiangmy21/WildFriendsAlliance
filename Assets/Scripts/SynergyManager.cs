using UnityEngine;
using System.Collections.Generic; // 需要用到列表 List

public class SynergyManager : MonoBehaviour
{
    // --- 单例 ---
    public static SynergyManager Instance { get; private set; }
    void Awake() { if (Instance == null) Instance = this; else Destroy(gameObject); }

    // --- UI 引用 ---
    [Header("羁绊UI引用")]
    public SynergyEmblemUI forestLeapEmblem; // “林间跃动”的纹章UI脚本

    // --- 内部状态 ---
    private bool isForestLeapActive = false;
    private List<UnitController> activeMonkeys = new List<UnitController>();
    private List<UnitController> activeSquirrels = new List<UnitController>();

    /// <summary>
    /// 【核心方法】每当场上单位变化时，由 GameManager 调用
    /// </summary>
    public void UpdateSynergies(List<UnitController> unitsOnField)
    {
        CheckForestLeap(unitsOnField);

        // TODO: 在这里添加其他羁绊的检测
        // CheckWetlandSynergy(unitsOnField);
    }

    /// <summary>
    /// 检测"林间跃动"羁绊
    /// </summary>
    private void CheckForestLeap(List<UnitController> unitsOnField)
    {
        List<UnitController> monkeys = new List<UnitController>();
        List<UnitController> squirrels = new List<UnitController>();

        // 1. 在场上寻找所有"猴子"和"松鼠"
        foreach (var unit in unitsOnField)
        {
            // 假设你的 UnitData 里有 unitName 字段
            if (unit.unitData.unitName == "金丝猴") monkeys.Add(unit);
            if (unit.unitData.unitName == "松鼠") squirrels.Add(unit);
        }

        // 2. 检查条件：至少有一只猴子和一只松鼠
        bool conditionMet = (monkeys.Count > 0 && squirrels.Count > 0);

        // 3. 根据条件更新状态
        if (conditionMet && !isForestLeapActive)
        {
            // 从"未激活"变为"激活"
            ActivateForestLeap(monkeys, squirrels);
        }
        else if (!conditionMet && isForestLeapActive)
        {
            // 从"激活"变为"未激活"
            DeactivateForestLeap();
        }
        else if (conditionMet && isForestLeapActive)
        {
            // 羁绊仍然激活，但可能有新的单位加入或离开
            UpdateForestLeapUnits(monkeys, squirrels);
        }
    }

    private void ActivateForestLeap(List<UnitController> monkeys, List<UnitController> squirrels)
    {
        Debug.Log($"羁绊【林间跃动】已激活！影响{monkeys.Count}只猴子和{squirrels.Count}只松鼠");
        isForestLeapActive = true;

        // 清空旧列表并添加新单位
        activeMonkeys.Clear();
        activeSquirrels.Clear();
        activeMonkeys.AddRange(monkeys);
        activeSquirrels.AddRange(squirrels);

        // a. 点亮UI
        forestLeapEmblem.Activate(true);

        // b. 对所有猴子施加常驻 Buff (提高闪避率)
        foreach (var monkey in activeMonkeys)
        {
            monkey.ApplyBuff("MissRate", 0.15f); // 假设提高15%闪避
            monkey.OnDodge += OnMonkeyDodge; // 订阅闪避事件
        }

        // c. 对所有松鼠施加常驻 Buff (提高闪避率)
        foreach (var squirrel in activeSquirrels)
        {
            squirrel.ApplyBuff("MissRate", 0.15f); // 假设提高15%闪避
            squirrel.OnDodge += OnSquirrelDodge; // 订阅闪避事件
        }
    }

    private void DeactivateForestLeap()
    {
        Debug.Log("羁绊【林间跃动】已失效！");
        isForestLeapActive = false;

        // a. 熄灭UI
        forestLeapEmblem.Activate(false);

        // b. 移除所有猴子的效果
        foreach (var monkey in activeMonkeys)
        {
            if (monkey != null)
            {
                monkey.RemoveBuff("MissRate");
                monkey.OnDodge -= OnMonkeyDodge; // 取消订阅
            }
        }

        // c. 移除所有松鼠的效果
        foreach (var squirrel in activeSquirrels)
        {
            if (squirrel != null)
            {
                squirrel.RemoveBuff("MissRate");
                squirrel.OnDodge -= OnSquirrelDodge; // 取消订阅
            }
        }

        // d. 清空列表
        activeMonkeys.Clear();
        activeSquirrels.Clear();
    }

    private void UpdateForestLeapUnits(List<UnitController> monkeys, List<UnitController> squirrels)
    {
        // 移除已经不在场上的单位的效果
        for (int i = activeMonkeys.Count - 1; i >= 0; i--)
        {
            if (!monkeys.Contains(activeMonkeys[i]))
            {
                // 这只猴子已经不在场上了
                var monkey = activeMonkeys[i];
                if (monkey != null)
                {
                    monkey.RemoveBuff("MissRate");
                    monkey.OnDodge -= OnMonkeyDodge;
                }
                activeMonkeys.RemoveAt(i);
            }
        }

        for (int i = activeSquirrels.Count - 1; i >= 0; i--)
        {
            if (!squirrels.Contains(activeSquirrels[i]))
            {
                // 这只松鼠已经不在场上了
                var squirrel = activeSquirrels[i];
                if (squirrel != null)
                {
                    squirrel.RemoveBuff("MissRate");
                    squirrel.OnDodge -= OnSquirrelDodge;
                }
                activeSquirrels.RemoveAt(i);
            }
        }

        // 为新加入的单位添加效果
        foreach (var monkey in monkeys)
        {
            if (!activeMonkeys.Contains(monkey))
            {
                monkey.ApplyBuff("MissRate", 0.15f);
                monkey.OnDodge += OnMonkeyDodge;
                activeMonkeys.Add(monkey);
            }
        }

        foreach (var squirrel in squirrels)
        {
            if (!activeSquirrels.Contains(squirrel))
            {
                squirrel.ApplyBuff("MissRate", 0.15f);
                squirrel.OnDodge += OnSquirrelDodge;
                activeSquirrels.Add(squirrel);
            }
        }
    }

    // --- 事件处理器 ---
    private void OnSquirrelDodge(UnitController squirrel)
    {
        Debug.Log("【林间跃动】松鼠闪避成功，回复生命！");
        squirrel.Heal(60);
    }

    private void OnMonkeyDodge(UnitController monkey)
    {
        Debug.Log("【林间跃动】猴子闪避成功，进行反击！");
        monkey.CounterAttack(60);
    }
}