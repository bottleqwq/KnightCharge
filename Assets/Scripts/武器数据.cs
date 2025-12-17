using UnityEngine;

/// <summary>
/// 武器数据类：存储每把武器的所有属性
/// </summary>
[System.Serializable]
public class 武器数据
{
    [Header("武器基本信息")]
    public string 武器名称;
    public Sprite 武器Sprite;

    [Header("旋转设置")]
    public float 旋转速度 = 300f;

    [Header("蓄力设置")]
    public float 最大蓄力时间 = 2.0f;

    [Header("冲刺设置")]
    public float 最小冲刺力度 = 1f;
    public float 最大冲刺力度 = 10f;

    [Header("伤害设置")]
    public float 基础伤害 = 10f;
    public float 蓄力加成伤害 = 20f;

    [Header("击退设置")]
    public float 击退力度 = 10f; // 对敌人造成的击退力度

    [Header("体力设置")]
    public float 体力消耗 = 10f;

    [Header("碰撞器设置")]
    public Vector2 碰撞器尺寸 = new Vector2(0.2f, 0.64f); // Box Collider2D的Size

    [Header("武器介绍")]
    [TextArea]
    public string 武器介绍;
    /// <summary>
    /// 构造函数：创建武器数据
    /// </summary>
    public 武器数据(string name, Sprite sprite)
    {
        武器名称 = name;
        武器Sprite = sprite;
    }
}