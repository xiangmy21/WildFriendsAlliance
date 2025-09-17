using UnityEngine; // 别忘了
using System.Collections; // 别忘了
using DG.Tweening;

public class UnitController : MonoBehaviour
{
    // 1. 数据
    [Header("Data Blueprint")]
    public UnitData unitData; // 在Inspector中拖入你的SO数据

    [Header("Team")]
    public bool isEnemyTeam; // 标记是敌是友

    public GameObject damageNumberPrefab;

    // 2. 运行时属性 (Runtime Stats)
    private int currentHP;
    private int currentMP;
    private int currentShield;

    // TODO: Buff/Debuff系统可能会修改这些"当前"属性
    private int currentATK;
    private int currentDEF;
    private float currentRange;
    private float currentMoveSpeed;
    private float currentAttackInterval;
    private bool isAttackOnCooldown; // 用于控制攻击冷却

    [Header("远程攻击 (Projectile)")]
    // 1. 把你的 "Pinecone_Prefab" 拖到这里
    public GameObject projectilePrefab;
    // 2. (重要) 创建一个空物体作为松鼠的“手”或“发射点”
    //    把它作为松鼠 Prefab 的子物体，并拖到这里
    public Transform projectileSpawnPoint;

    // 3. 状态管理
    private enum UnitState { Idle, Seeking, Moving, Attacking, Stunned, Dead }
    private UnitState currentState;

    private UnitController currentTarget;
    private float attackTimer;

    // 动画控制器引用
    private Animator animator;

    // 4. 初始化
    void Start()
    {
        // 从SO初始化属性
        InitializeFromData();
        currentState = UnitState.Idle;
        animator = GetComponent<Animator>();
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
        currentAttackInterval = unitData.baseAttackInterval;

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
                animator.SetBool("isMoving", false); // 告诉 Animator 停止移动动画
                FindNewTarget();
                if (currentTarget != null)
                {
                    currentState = UnitState.Moving;
                }
                break;

            case UnitState.Moving:
                animator.SetBool("isMoving", true); // 告诉 Animator 播放移动动画
                if (currentTarget == null || currentTarget.currentState == UnitState.Dead)
                {
                    currentState = UnitState.Idle;
                    break;
                }

                if (IsTargetInRange())
                {
                    // 停止移动
                    currentState = UnitState.Attacking;
                    attackTimer = currentAttackInterval; // 切换到攻击时立刻准备
                }
                else
                {
                    // 朝目标移动
                    MoveTowardsTarget();
                }
                break;

            case UnitState.Attacking:
                animator.SetBool("isMoving", false); // 攻击时通常不移动
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

                if (isAttackOnCooldown)
                {
                    return;
                }
                isAttackOnCooldown = true;
                ExecuteAttackOrSkill();
                StartCoroutine(AttackCooldownRoutine(currentAttackInterval));
                break;
        }
    }

    // 这个协程现在只负责“计时”，不负责伤害
    IEnumerator AttackCooldownRoutine(float duration)
    {
        yield return new WaitForSeconds(duration);
        isAttackOnCooldown = false; // 冷却结束，允许下一次攻击
    }

    // 6. 核心行为

    void FindNewTarget()
    {
        // 在感知范围内寻找敌人，使用2D物理检测
        float detectionRange = currentRange * 20f;
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, detectionRange);

        UnitController closestEnemy = null;
        float closestDistance = float.MaxValue;

        foreach (Collider2D col in colliders)
        {
            UnitController otherUnit = col.GetComponent<UnitController>();
            if (otherUnit != null &&
                otherUnit != this &&
                otherUnit.isEnemyTeam != this.isEnemyTeam &&
                otherUnit.currentState != UnitState.Dead)
            {
                float distance = Vector2.Distance(transform.position, otherUnit.transform.position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestEnemy = otherUnit;
                }
            }
        }

        currentTarget = closestEnemy;
    }

    void MoveTowardsTarget()
    {
        if (currentTarget == null) return;

        Vector3 direction = (currentTarget.transform.position - transform.position).normalized;
        float distanceToMove = currentMoveSpeed * Time.deltaTime;

        transform.position += direction * distanceToMove;
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
        animator.SetTrigger("doAttack"); // 触发攻击动画
    }

    public void OnAttackHit() // 动画播放至伤害触发
    {
        if (currentTarget != null && currentState == UnitState.Attacking)
        {
        Debug.Log($"{name} 攻击了 {currentTarget.name}!");
        currentTarget.TakeDamage(currentATK);
        // 攻击加蓝
        GainMP(unitData.mpGainOnAttack);
    }
    }

    /// <summary>
    /// 动画事件：在动画的“发射帧”调用
    /// </summary>
    public void OnLaunchProjectile()
    {
        // 目标在发射瞬间丢失，就不发射
        if (currentTarget == null)
        {
            return;
        }

        // 检查 Prefab 是否设置
        if (projectilePrefab == null || projectileSpawnPoint == null)
        {
            Debug.LogError($"{name} 想要发射抛射物, 但 prefab 或 spawnPoint 未设置！");
            return;
        }

        // 1. 在“发射点”生成“松果”
        GameObject projGO = Instantiate(projectilePrefab, projectileSpawnPoint.position, Quaternion.identity);

        // 2. 获取松果的 Projectile 脚本
        Projectile projectileScript = projGO.GetComponent<Projectile>();

        // 3. “发射！” - 把目标和伤害告诉松果
        if (projectileScript != null)
        {
            projectileScript.Fire(currentTarget, currentATK);
            GainMP(unitData.mpGainOnAttack);
        }
        else
        {
            Debug.LogError("抛射物 Prefab 上没有找到 Projectile.cs 脚本！");
        }
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
            var spriteRenderer = GetComponent<SpriteRenderer>();
#if !UNITY_WEBGL || UNITY_EDITOR
            // 0.1秒内闪红，然后0.1秒内恢复
            // Yoyo(1) 表示播放一次然后倒放一次
            spriteRenderer.DOColor(Color.red, 0.1f).SetLoops(2, LoopType.Yoyo);
            // 让 transform 在 0.2 秒内，在 x 和 y 轴上抖动，强度为 0.1
            // 最后一个参数 vibrato (振动) 调高点，抖动频率会更高
            transform.DOShakePosition(duration: 0.2f, strength: 0.1f, vibrato: 20);
#else
            // WebGL平台使用简单的颜色变化
            StartCoroutine(SimpleFlashEffect(spriteRenderer));
#endif

            GainMP(unitData.mpGainOnHit);

            if (damageNumberPrefab != null) // 伤害数字显示
            {
                // 1. 在角色的位置实例化 Prefab
                // (你可能想在头顶加一点偏移量)
                Vector3 spawnPos = transform.position + new Vector3(0.2f, 0.5f, 0); // 比如在Y轴上方0.5个单位

                // (可选) 增加一点随机偏移，防止数字完全重叠
                spawnPos += new Vector3(Random.Range(-0.3f, 0.3f), 0, 0);

                GameObject textInstance = Instantiate(damageNumberPrefab, spawnPos, Quaternion.identity);

                // 2. 获取脚本并调用方法
                DamageNumberEffect effectScript = textInstance.GetComponent<DamageNumberEffect>();
                if (effectScript != null)
                {
                    // 3. 把伤害值传过去，启动动画！
                    effectScript.ShowDamage(damageTaken);
                }
            }
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

    void OnMouseDown()
    {
        // 检查是否是右键 (1代表右键)
        if (Input.GetMouseButtonDown(1))
        {
            // 战斗中不允许回收
            if (GameManager.Instance.IsBattleActive) return;

            // 尝试把自己加回备战席
            bool success = BenchManager.Instance.AddUnitToBench(this.unitData);

            if (success)
            {
                // 加回成功，销毁自己
                Destroy(gameObject);
            }
            else
            {
                Debug.Log("备战席满了，无法回收！");
                // TODO: 可以在单位头上冒个泡，提示“备战席已满”
            }
        }
    }

#if UNITY_WEBGL && !UNITY_EDITOR
    // WebGL平台的简单闪烁效果
    private IEnumerator SimpleFlashEffect(SpriteRenderer spriteRenderer)
    {
        Color originalColor = spriteRenderer.color;
        spriteRenderer.color = Color.red;
        yield return new WaitForSeconds(0.1f);
        spriteRenderer.color = originalColor;
        yield return new WaitForSeconds(0.1f);
        spriteRenderer.color = Color.red;
        yield return new WaitForSeconds(0.1f);
        spriteRenderer.color = originalColor;
    }
#endif
}