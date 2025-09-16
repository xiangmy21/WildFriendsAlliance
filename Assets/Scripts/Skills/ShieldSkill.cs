using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ShieldSkill", menuName = "WildFriends/Skills/ShieldSkill")]
public class ShieldSkill : SkillBase
{
    public int shieldAmount = 120;

    public override void Activate(UnitController caster)
    {
        Debug.Log($"{caster.name} 释放了 {skillName}！");
        // 假设 UnitController 有一个 "AddShield" 方法
        caster.AddShield(shieldAmount);
    }
}