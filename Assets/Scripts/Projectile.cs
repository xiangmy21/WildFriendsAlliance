using UnityEngine;
using System.Collections;

public class Projectile : MonoBehaviour
{
    [Header("飞行设置")]
    // public float speed = 10f; // <--- 删掉或注释掉这一行
    public float travelDuration = 1.0f; // <--- 新增！设置松果飞完全程需要的固定时间（秒）
    public AnimationCurve arcCurve; // 【关键】在 Inspector 里编辑这个曲线，让它中间凸起，形成抛物线
    public string typename;
    public GameObject hitEffectPrefab; // 可选：命中时的特效预制体

    // --- 内部变量 ---
    private UnitController target;
    private int damage;
    private Vector3 startPosition;
    private Vector3 lastPosition; // 记录上一帧的位置，用于计算飞行方向
    private float travelTime;

    /// <summary>
    /// “发射”指令，由攻击者 (松鼠) 的 Animation Event 调用
    /// </summary>
    public void Fire(UnitController targetToChase, int damageToDeal)
    {
        this.target = targetToChase;
        this.damage = damageToDeal;
        this.startPosition = transform.position; // 记录起始位置
        this.lastPosition = transform.position; // 初始化上一帧位置
        this.travelTime = 0f;
    }

    void Update()
    {
        // 1. 目标丢失 (比如敌人中途死了)
        if (target == null)
        {
            Destroy(gameObject);
            return;
        }

        // 2. 【修改】累加“已用时间”
        travelTime += Time.deltaTime;

        // 3. 【核心修改】计算飞行进度 (t)
        // t 不再跟 speed 或 distance 挂钩
        // t 现在就是“已用时间”占“总时长”的百分比
        float t = travelTime / travelDuration;

        // 4. 计算从“起点”到“目标”的直线插值
        Vector3 targetPosition = target.transform.position; // 实时追踪目标
        Vector3 currentPosOnLine = Vector3.Lerp(startPosition, targetPosition, t);

        // 5. 【抛物线核心】(这部分不变)
        float arcHeight = arcCurve.Evaluate(t);
        currentPosOnLine.y += arcHeight;

        transform.position = currentPosOnLine;

        // 让子弹朝向飞行方向（假定子弹初始为横向图片）
        Vector3 direction = (currentPosOnLine - lastPosition).normalized;
        if (direction.magnitude > 0.01f) // 避免除零错误
        {
            // 计算角度（弧度转角度），子弹图片假定为横向（0度朝右）
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, 0, angle);
        }

        // 更新上一帧位置
        lastPosition = currentPosOnLine;

        // 5. 检查是否抵达 (t >= 1)
        if (t >= 1f)
        {
            HitTarget();
        }
    }

    /// <summary>
    /// 当 2D 触发器 (Is Trigger) 碰撞时调用
    /// </summary>
    void OnTriggerEnter2D(Collider2D other)
    {
        // 检查是不是我们想要命中的那个目标
        if (target != null && other.gameObject == target.gameObject)
        {
            HitTarget();
        }
    }

    /// <summary>
    /// 命中目标的统一处理
    /// </summary>
    void HitTarget()
    {
        if (target != null)
        {
            // 1. 造成伤害
            target.TakeDamage(damage);
            // 2. 处理特殊效果
            if (typename == "stun") //晕眩
            {
                // 立即施加晕眩
                target.ApplyStun(true);

                // 生成晕眩特效在敌人上方
                if (hitEffectPrefab != null)
                {
                    Vector3 effectPosition = target.transform.position + new Vector3(0, 0.5f, 0);
                    GameObject stunEffect = Instantiate(hitEffectPrefab, effectPosition, Quaternion.identity);

                    // 让特效跟随敌人
                    stunEffect.transform.SetParent(target.transform);

                    // 给特效添加自动销毁脚本，负责恢复敌人状态
                    StunEffectController stunController = stunEffect.AddComponent<StunEffectController>();
                    stunController.Initialize(target, 2.5f);
                }
                else
                {
                    // 没有特效时，让敌人自己处理晕眩恢复
                    target.StartStunRecovery(2.5f);
                }
            }
        }

        // 2. (可选) 在此位置生成一个“击中特效”
        // Instantiate(hitEffectPrefab, transform.position, Quaternion.identity);

        // 3. 销毁松果自己
        Destroy(gameObject);
    }
}