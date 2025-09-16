using UnityEngine; // 别忘了
using System.Collections; // 别忘了

public class UnitController : MonoBehaviour
{
    // 1. 数据
    [Header("Data Blueprint")]
    public UnitData unitData; // 在Inspector中拖入你的SO数据

    [Header("Team")]
    public bool isEnemyTeam; // 标记是敌是友

    // 2. 运行时属性 (Runtime Stats)
    private int currentHP;
    private int currentMP;
    private int currentShield;

    // TODO: Buff/Debuff系统可能会修改这些"当前"属性
    private int currentATK;
    private int currentDEF;
    private float currentRange;
    private float currentMoveSpeed;
    private float currentAttackFrequency;

    // 3. 状态管理
    private enum UnitState { Idle, Seeking, Moving, Attacking, Stunned, Dead }
    private UnitState currentState;

    private UnitController currentTarget;
    private float attackTimer;

    // 4. 初始化
    void Start()
    {
        // 从SO初始化属性
        InitializeFromData();
        currentState = UnitState.Idle;
    }

    public void InitializeFromData()
    {
        currentHP = unitData.maxHP;
        currentMP = 0;
        currentShield = 0;

        currentATK = unitData.baseATK;
        currentDEF = unitData.baseDEF;
        currentRange = unitData.baseRange;
        currentMoveSpeed = unitData.baseMoveSpeed;
        currentAttackFrequency = unitData.baseAttackFrequency;

        // TODO: 更新血条和蓝条UI
    }

    // 5. 核心Update循环 (状态机)
    void Update()
    {
        // 游戏未开始或被眩晕、死亡，则不执行任何逻辑
        if (currentState == UnitState.Stunned || currentState == UnitState.Dead || !GameManager.Instance.IsBattleActive)
        {
            return;
        }

        switch (currentState)
        {
            case UnitState.Idle:
                FindNewTarget();
                if (currentTarget != null)
                {
                    currentState = UnitState.Moving;
                }
                break;

            case UnitState.Moving:
                if (currentTarget == null || currentTarget.currentState == UnitState.Dead)
                {
                    currentState = UnitState.Idle;
                    break;
                }

                if (IsTargetInRange())
                {
                    // 停止移动
                    currentState = UnitState.Attacking;
                    attackTimer = 0f; // 切换到攻击时立刻准备
                }
                else
                {
                    // 朝目标移动
                    MoveTowardsTarget();
                }
                break;

            case UnitState.Attacking:
                if (currentTarget == null || currentTarget.currentState == UnitState.Dead)
                {
                    currentState = UnitState.Idle;
                    break;
                }

                if (!IsTargetInRange())
                {
                    currentState = UnitState.Moving;
                    break;
                }

                // 攻击计时
                attackTimer += Time.deltaTime;
                float attackCooldown = 1.0f / currentAttackFrequency;

                if (attackTimer >= attackCooldown)
                {
                    attackTimer -= attackCooldown; // 减去冷却，而不是归零，防止攻速溢出
                    ExecuteAttackOrSkill();
                }
                break;
        }
    }

    // 6. 核心行为

    void FindNewTarget()
    {
        // TODO: 在这里写你的索敌逻辑
        // 1. Physics.OverlapSphere 找到所有Collider
        // 2. 遍历并 GetComponent<UnitController>()
        // 3. 找到 isEnemyTeam != this.isEnemyTeam 且最近的单位
        // 4. 设置 currentTarget
    }

    void MoveTowardsTarget()
    {
        // TODO: 在这里写你的移动逻辑
        // 1. transform.position = Vector3.MoveTowards(...)
        // 2. 或者 agent.SetDestination(currentTarget.transform.position) (如果用NavMesh)
        // 3. 始终朝向敌人 transform.LookAt(currentTarget.transform)
    }

    bool IsTargetInRange()
    {
        if (currentTarget == null) return false;
        return Vector3.Distance(transform.position, currentTarget.transform.position) <= currentRange;
    }

    void ExecuteAttackOrSkill()
    {
        // 关键逻辑：检查MP
        if (currentMP >= unitData.maxMP)
        {
            // 释放技能
            if (unitData.skillLogic != null)
            {
                unitData.skillLogic.Activate(this);
                currentMP = 0; // MP清零
            }
            else
            {
                // 没技能就普攻
                PerformBasicAttack();
            }
        }
        else
        {
            // 普攻
            PerformBasicAttack();
        }

        // TODO: 更新MP UI
    }

    void PerformBasicAttack()
    {
        if (currentTarget == null) return;

        Debug.Log($"{name} 攻击了 {currentTarget.name}!");
        currentTarget.TakeDamage(currentATK);

        // 攻击加蓝
        GainMP(unitData.mpGainOnAttack);
    }

    // 7. 公共API (给其他人调用)

    public void TakeDamage(int damageAmount)
    {
        // 伤害计算公式
        int damageTaken = Mathf.Max(damageAmount - currentDEF, 0);

        // 护盾逻辑
        if (currentShield > 0)
        {
            if (currentShield >= damageTaken)
            {
                currentShield -= damageTaken;
                damageTaken = 0;
            }
            else
            {
                damageTaken -= currentShield;
                currentShield = 0;
            }
        }

        currentHP -= damageTaken;

        // 受伤加蓝
        if (damageTaken > 0)
        {
            GainMP(unitData.mpGainOnHit);
        }

        // TODO: 更新HP UI

        if (currentHP <= 0 && currentState != UnitState.Dead)
        {
            Die();
        }
    }

    public void GainMP(int amount)
    {
        if (currentState == UnitState.Dead) return;
        currentMP = Mathf.Min(currentMP + amount, unitData.maxMP);
        // TODO: 更新MP UI
    }

    public void AddShield(int amount)
    {
        currentShield += amount;
        Debug.Log($"{name} 获得了 {amount} 点护盾, 总护盾: {currentShield}");
    }

    public void ApplyStun(bool isStunned)
    {
        if (isStunned)
        {
            currentState = UnitState.Stunned;
            Debug.Log($"{name} 被眩晕了!");
            // TODO: 停止移动, 停止动画
        }
        else
        {
            // 眩晕结束后，重置回Idle，状态机会自动重新索敌
            currentState = UnitState.Idle;
            Debug.Log($"{name} 眩晕结束!");
        }
    }

    void Die()
    {
        currentState = UnitState.Dead;
        Debug.Log($"{name} 阵亡了!");
        // TODO: 播放死亡动画
        // TODO: 通知GameManager
        Destroy(gameObject, 2.0f); // 2秒后移除尸体
    }

    // 辅助方法，给技能用
    public UnitController GetCurrentTarget()
    {
        return currentTarget;
    }
}