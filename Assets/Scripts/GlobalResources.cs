using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 全局资源管理单例 - 用于存储和提供游戏中使用的图片资源
/// </summary>
public class GlobalResources : MonoBehaviour
{
    public static GlobalResources Instance { get; private set; }

    [Header("UI背景图片")]
    public Sprite questionBackground;
    public Sprite cardBackground;
    public Sprite buttonBackground;

    [Header("动物头像")]
    public Sprite squirrelAvatar;
    public Sprite hedgehogAvatar;
    public Sprite boarAvatar;
    public Sprite owlAvatar;
    public Sprite foxAvatar;
    public Sprite redPandaAvatar;
    public Sprite deerAvatar;
    public Sprite monkeyAvatar;
    public Sprite pandaAvatar;
    public Sprite tigerAvatar;

    [Header("动物卡片")]
    public Sprite squirrelCard;
    public Sprite hedgehogCard;
    public Sprite boarCard;
    public Sprite owlCard;
    public Sprite foxCard;
    public Sprite redPandaCard;
    public Sprite deerCard;
    public Sprite monkeyCard;
    public Sprite pandaCard;
    public Sprite tigerCard;

    [Header("图标资源")]
    public Sprite correctIcon;
    public Sprite wrongIcon;
    public Sprite starIcon;
    public Sprite heartIcon;

    [Header("效果图片")]
    public Sprite[] skillEffects;
    public Sprite[] battleEffects;

    // 动物名称到头像的映射
    private Dictionary<string, Sprite> animalAvatarMap;
    // 动物名称到卡片的映射
    private Dictionary<string, Sprite> animalCardMap;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeMappings();
            Debug.Log("[GlobalResources] 全局资源管理器初始化完成");
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void InitializeMappings()
    {
        // 初始化动物头像映射
        animalAvatarMap = new Dictionary<string, Sprite>()
        {
            {"松鼠", squirrelAvatar},
            {"刺猬", hedgehogAvatar},
            {"野猪", boarAvatar},
            {"猫头鹰", owlAvatar},
            {"赤狐", foxAvatar},
            {"小熊猫", redPandaAvatar},
            {"梅花鹿", deerAvatar},
            {"金丝猴", monkeyAvatar},
            {"大熊猫", pandaAvatar},
            {"东北虎", tigerAvatar}
        };

        // 初始化动物卡片映射
        animalCardMap = new Dictionary<string, Sprite>()
        {
            {"松鼠", squirrelCard},
            {"刺猬", hedgehogCard},
            {"野猪", boarCard},
            {"猫头鹰", owlCard},
            {"赤狐", foxCard},
            {"小熊猫", redPandaCard},
            {"梅花鹿", deerCard},
            {"金丝猴", monkeyCard},
            {"大熊猫", pandaCard},
            {"东北虎", tigerCard}
        };
    }

    // 获取动物头像
    public Sprite GetAnimalAvatar(string animalName)
    {
        if (animalAvatarMap != null && animalAvatarMap.ContainsKey(animalName))
        {
            return animalAvatarMap[animalName];
        }

        Debug.LogWarning($"[GlobalResources] 未找到动物 {animalName} 的头像");
        return null;
    }

    // 获取动物卡片
    public Sprite GetAnimalCard(string animalName)
    {
        if (animalCardMap != null && animalCardMap.ContainsKey(animalName))
        {
            return animalCardMap[animalName];
        }

        Debug.LogWarning($"[GlobalResources] 未找到动物 {animalName} 的卡片");
        return null;
    }

    // 获取UI背景
    public Sprite GetUIBackground(UIBackgroundType type)
    {
        switch (type)
        {
            case UIBackgroundType.Question:
                return questionBackground;
            case UIBackgroundType.Card:
                return cardBackground;
            case UIBackgroundType.Button:
                return buttonBackground;
            default:
                return null;
        }
    }

    // 获取图标
    public Sprite GetIcon(IconType type)
    {
        switch (type)
        {
            case IconType.Correct:
                return correctIcon;
            case IconType.Wrong:
                return wrongIcon;
            case IconType.Star:
                return starIcon;
            case IconType.Heart:
                return heartIcon;
            default:
                return null;
        }
    }

    // 获取技能效果图片
    public Sprite GetSkillEffect(int index)
    {
        if (skillEffects != null && index >= 0 && index < skillEffects.Length)
        {
            return skillEffects[index];
        }
        return null;
    }

    // 获取战斗效果图片
    public Sprite GetBattleEffect(int index)
    {
        if (battleEffects != null && index >= 0 && index < battleEffects.Length)
        {
            return battleEffects[index];
        }
        return null;
    }

    // 检查资源是否已加载
    public bool IsResourceLoaded(string animalName)
    {
        return animalAvatarMap != null &&
               animalAvatarMap.ContainsKey(animalName) &&
               animalAvatarMap[animalName] != null;
    }

    // 获取所有可用的动物名称
    public string[] GetAllAnimalNames()
    {
        if (animalAvatarMap != null)
        {
            string[] names = new string[animalAvatarMap.Count];
            animalAvatarMap.Keys.CopyTo(names, 0);
            return names;
        }
        return new string[0];
    }

    // 资源预加载（可在游戏启动时调用）
    public void PreloadResources()
    {
        Debug.Log("[GlobalResources] 开始预加载资源...");

        // 这里可以添加通过Resources.Load或AssetBundle加载资源的逻辑
        // 例如：
        // if (questionBackground == null)
        //     questionBackground = Resources.Load<Sprite>("UI/questionbackground");

        Debug.Log("[GlobalResources] 资源预加载完成");
    }

    #if UNITY_EDITOR
    // 编辑器下的资源检查
    [ContextMenu("检查资源完整性")]
    void CheckResourceIntegrity()
    {
        int missingCount = 0;

        Debug.Log("=== 资源完整性检查 ===");

        // 检查UI背景
        if (questionBackground == null) { Debug.LogWarning("缺少: questionBackground"); missingCount++; }
        if (cardBackground == null) { Debug.LogWarning("缺少: cardBackground"); missingCount++; }
        if (buttonBackground == null) { Debug.LogWarning("缺少: buttonBackground"); missingCount++; }

        // 检查动物头像
        string[] animals = {"松鼠", "刺猬", "野猪", "猫头鹰", "赤狐", "小熊猫", "梅花鹿", "金丝猴", "大熊猫", "东北虎"};
        foreach (string animal in animals)
        {
            if (GetAnimalAvatar(animal) == null) missingCount++;
            if (GetAnimalCard(animal) == null) missingCount++;
        }

        // 检查图标
        if (correctIcon == null) { Debug.LogWarning("缺少: correctIcon"); missingCount++; }
        if (wrongIcon == null) { Debug.LogWarning("缺少: wrongIcon"); missingCount++; }
        if (starIcon == null) { Debug.LogWarning("缺少: starIcon"); missingCount++; }
        if (heartIcon == null) { Debug.LogWarning("缺少: heartIcon"); missingCount++; }

        if (missingCount == 0)
        {
            Debug.Log("✓ 所有资源完整");
        }
        else
        {
            Debug.LogWarning($"✗ 缺少 {missingCount} 个资源");
        }
    }
    #endif
}

// UI背景类型枚举
public enum UIBackgroundType
{
    Question,
    Card,
    Button
}

// 图标类型枚举
public enum IconType
{
    Correct,
    Wrong,
    Star,
    Heart
}