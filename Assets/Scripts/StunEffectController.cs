using UnityEngine;
using System.Collections;

public class StunEffectController : MonoBehaviour
{
    private UnitController targetUnit;
    private float stunDuration;

    public void Initialize(UnitController target, float duration)
    {
        targetUnit = target;
        stunDuration = duration;
        StartCoroutine(StunRecoveryCoroutine());
    }

    IEnumerator StunRecoveryCoroutine()
    {
        // 等待晕眩时间
        yield return new WaitForSeconds(stunDuration);

        // 恢复敌人状态
        if (targetUnit != null)
        {
            targetUnit.ApplyStun(false);
        }

        // 销毁特效自己
        Destroy(gameObject);
    }
}