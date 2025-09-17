using UnityEngine;

public class Projectile : MonoBehaviour
{
    [Header("飞行设置")]
    public float speed = 10f; // 飞行的基础速度
    public AnimationCurve arcCurve; // 【关键】在 Inspector 里编辑这个曲线，让它中间凸起，形成抛物线

    // --- 内部变量 ---
    private UnitController target;
    private int damage;
    private Vector3 startPosition;
    private float travelTime;

    /// <summary>
    /// “发射”指令，由攻击者 (松鼠) 的 Animation Event 调用
    /// </summary>
    public void Fire(UnitController targetToChase, int damageToDeal)
    {
        this.target = targetToChase;
        this.damage = damageToDeal;
        this.startPosition = transform.position; // 记录起始位置
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

        // 2. 计算飞行
        travelTime += Time.deltaTime * speed;

        // 3. 计算从“起点”到“目标”的直线插值
        Vector3 targetPosition = target.transform.position; // 实时追踪目标
        float distance = Vector3.Distance(startPosition, targetPosition);

        // 如果距离很近，直接命中 (防止除以零)
        if (distance < 0.1f)
        {
            HitTarget();
            return;
        }

        // 计算当前在直线上的位置 (t 是 0 到 1 的百分比)
        float t = travelTime / distance;
        Vector3 currentPosOnLine = Vector3.Lerp(startPosition, targetPosition, t);

        // 4. 【抛物线核心】
        // 根据 AnimationCurve (我们设置的抛物线) 在 Y 轴上增加高度
        float arcHeight = arcCurve.Evaluate(t); // Evaluate(t) 会返回曲线在 t (0-1) 位置的高度
        currentPosOnLine.y += arcHeight; // 在直线上增加高度

        transform.position = currentPosOnLine;

        // (可选) 让松果朝向飞行方向
        // Vector3 direction = (currentPosOnLine - transform.position).normalized;
        // ... (计算旋转的代码) ...

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
        }

        // 2. (可选) 在此位置生成一个“击中特效”
        // Instantiate(hitEffectPrefab, transform.position, Quaternion.identity);

        // 3. 销毁松果自己
        Destroy(gameObject);
    }
}