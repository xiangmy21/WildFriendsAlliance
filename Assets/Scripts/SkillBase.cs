using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 这是一个抽象类，它本身不能被创建，但可以被继承
public abstract class SkillBase : ScriptableObject
{
    public string skillName;
    [TextArea]
    public string skillDescription;

    // 关键方法：当技能被释放时，UnitController会调用这个方法
    // 我们把 "caster" (施法者) 传进去，以便技能逻辑可以控制施法者
    // (比如：给自己加盾，或开启一个协程)
    public abstract void Activate(UnitController caster);
}