// 资产菜单 -> Create -> WildFriends -> UnitData
using UnityEngine;

[CreateAssetMenu(fileName = "NewUnitData", menuName = "WildFriends/UnitData")]
public class UnitData : ScriptableObject
{
    [Header("Basic Info")]
    public string unitName;
    public GameObject unitPrefab; // 角色模型/动画的Prefab

    [Header("Core Stats")]
    public int maxHP;
    public int maxMP;
    public int baseATK;
    public int baseDEF;
    public float baseRange;       // 攻击范围
    public float baseMoveSpeed;
    public float baseAttackFrequency; // 攻击频率 (例如: 1.2，代表每秒1.2次)

    [Header("MP & Skill")]
    public int mpGainOnAttack; // 攻击时获取的MP
    public int mpGainOnHit;    // 受伤时获取的MP

    // 核心：这个单位绑定的技能逻辑
    public SkillBase skillLogic;
}