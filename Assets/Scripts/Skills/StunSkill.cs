using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "StunSkill", menuName = "WildFriends/Skills/StunSkill")]
public class StunSkill : SkillBase
{
    public float stunDuration = 1.5f;

    public override void Activate(UnitController caster)
    {
        // 假设 UnitController 有一个 "GetCurrentTarget()" 方法
        UnitController target = caster.GetCurrentTarget();

        if (target != null)
        {
            Debug.Log($"{caster.name} 对 {target.name} 释放了 {skillName}！");
            // "caster" 是一个 MonoBehaviour，所以它可以启动协程
            caster.StartCoroutine(StunTargetCoroutine(target));
        }
    }

    private IEnumerator StunTargetCoroutine(UnitController target)
    {
        // 假设 UnitController 有一个 "isStunned" 状态
        target.ApplyStun(true);
        yield return new WaitForSeconds(stunDuration);

        // 确保目标还活着
        if (target != null)
        {
            target.ApplyStun(false);
        }
    }
}
