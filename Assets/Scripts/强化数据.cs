using UnityEngine;

// 定义枚举：用来区分升级的类型
public enum UpgradeType
{
    生命值强化,
    体力值强化,
    护甲值强化,
    护甲恢复延迟强化,
    护甲恢复速度强化,
    护盾时长强化,
    体力值恢复速度强化,
    护盾爆率强化,
    血瓶爆率强化,
    金币爆率强化,
    祝福固定增长强化,
    祝福击杀增长强化,
    拾取范围强化,
    生命窃取率强化,
    闪避率强化,
    基础伤害强化,
    蓄力伤害强化,
    转速减慢,
    转速加快,
    击退强化,
    冲刺力度强化,
    血瓶恢复值强化,
    生命窃取值强化
}
//定义稀有度枚举
public enum Rarity
{
    普通,
    稀有,
    史诗
}
// [CreateAssetMenu] 允许你在 Unity 的 Project 窗口右键创建这个数据文件
[CreateAssetMenu(fileName = "新的强化", menuName = "Game/创建强化")]
public class UpgradeData : ScriptableObject
{
    [Header("UI 显示信息")]
    public string 强化名称;
    [TextArea]
    public string 强化介绍;
    public Sprite 强化图标;

    [Header("稀有度设置")]
    public Rarity rarity;

    [Header("逻辑属性")]
    public UpgradeType type;
    public float 数值;

    [Header("限制设置")]
    public int maxCount = -1; // 【新】最大选择次数。-1 代表无限次（如加攻击力），3 代表只能选3次
}