using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/// <summary>
/// 武器选择UI控制器：处理武器选择界面的交互
/// </summary>
public class 武器选择UI控制器 : MonoBehaviour
{
    [Header("武器按钮设置")]
    public Button 新手木剑按钮;
    public Button 铁质长剑按钮;
    public Button 黄金巨剑按钮;

    [Header("武器Sprite引用")]
    public Sprite 新手木剑Sprite;
    public Sprite 铁质长剑Sprite;
    public Sprite 黄金巨剑Sprite;

    [Header("新手木剑属性")]
    public float 新手木剑_旋转速度 = 250f;
    public float 新手木剑_最大蓄力时间 = 2.5f;
    public float 新手木剑_最小冲刺力度 = 0.8f;
    public float 新手木剑_最大冲刺力度 = 8f;
    public float 新手木剑_基础伤害 = 8f;
    public float 新手木剑_蓄力加成伤害 = 15f;
    public float 新手木剑_击退力度 = 8f;
    public float 新手木剑_体力消耗 = 8f;
    public float 新手木剑_体力恢复 = 12f;
    public Vector2 新手木剑_碰撞器尺寸 = new Vector2(0.3f, 0.8f); // 较大，容易命中

    [Header("铁质长剑属性")]
    public float 铁质长剑_旋转速度 = 320f;
    public float 铁质长剑_最大蓄力时间 = 2.0f;
    public float 铁质长剑_最小冲刺力度 = 1.0f;
    public float 铁质长剑_最大冲刺力度 = 10f;
    public float 铁质长剑_基础伤害 = 12f;
    public float 铁质长剑_蓄力加成伤害 = 20f;
    public float 铁质长剑_击退力度 = 12f;
    public float 铁质长剑_体力消耗 = 10f;
    public float 铁质长剑_体力恢复 = 15f;
    public Vector2 铁质长剑_碰撞器尺寸 = new Vector2(0.2f, 0.64f); // 标准大小

    [Header("黄金巨剑属性")]
    public float 黄金巨剑_旋转速度 = 300f;
    public float 黄金巨剑_最大蓄力时间 = 2.0f;
    public float 黄金巨剑_最小冲刺力度 = 20f;
    public float 黄金巨剑_最大冲刺力度 = 40f;
    public float 黄金巨剑_基础伤害 = 50f;
    public float 黄金巨剑_蓄力加成伤害 = 50f;
    public float 黄金巨剑_击退力度 = 30f;
    public float 黄金巨剑_体力消耗 = 1f;
    public float 黄金巨剑_体力恢复 = 15f;
    public Vector2 黄金巨剑_碰撞器尺寸 = new Vector2(0.15f, 0.5f); // 较小，难命中但伤害高

    [Header("UI设置")]
    public Button 开始游戏按钮;
    public string 游戏战斗场景名称 = "游戏战斗场景"; // 战斗场景的名称

    private 武器数据 当前选择的武器数据 = null;

    void Start()
    {
        // 绑定武器选择按钮
        if (新手木剑按钮 != null)
        {
            新手木剑按钮.onClick.AddListener(() => 选择武器(创建新手木剑数据()));
        }
        if (铁质长剑按钮 != null)
        {
            铁质长剑按钮.onClick.AddListener(() => 选择武器(创建铁质长剑数据()));
        }
        if (黄金巨剑按钮 != null)
        {
            黄金巨剑按钮.onClick.AddListener(() => 选择武器(创建黄金巨剑数据()));
        }

        // 绑定开始游戏按钮
        if (开始游戏按钮 != null)
        {
            开始游戏按钮.onClick.AddListener(开始游戏);
        }

        // 初始状态：开始按钮可能应该禁用，直到选择了武器
        if (开始游戏按钮 != null)
        {
            开始游戏按钮.interactable = false;
        }
    }

    /// <summary>
    /// 创建新手木剑的武器数据
    /// </summary>
    武器数据 创建新手木剑数据()
    {
        武器数据 数据 = new 武器数据("新手木剑", 新手木剑Sprite);
        数据.旋转速度 = 新手木剑_旋转速度;
        数据.最大蓄力时间 = 新手木剑_最大蓄力时间;
        数据.最小冲刺力度 = 新手木剑_最小冲刺力度;
        数据.最大冲刺力度 = 新手木剑_最大冲刺力度;
        数据.基础伤害 = 新手木剑_基础伤害;
        数据.蓄力加成伤害 = 新手木剑_蓄力加成伤害;
        数据.击退力度 = 新手木剑_击退力度;
        数据.体力消耗 = 新手木剑_体力消耗;
        数据.体力恢复 = 新手木剑_体力恢复;
        数据.碰撞器尺寸 = 新手木剑_碰撞器尺寸;
        return 数据;
    }

    /// <summary>
    /// 创建铁质长剑的武器数据
    /// </summary>
    武器数据 创建铁质长剑数据()
    {
        武器数据 数据 = new 武器数据("铁质长剑", 铁质长剑Sprite);
        数据.旋转速度 = 铁质长剑_旋转速度;
        数据.最大蓄力时间 = 铁质长剑_最大蓄力时间;
        数据.最小冲刺力度 = 铁质长剑_最小冲刺力度;
        数据.最大冲刺力度 = 铁质长剑_最大冲刺力度;
        数据.基础伤害 = 铁质长剑_基础伤害;
        数据.蓄力加成伤害 = 铁质长剑_蓄力加成伤害;
        数据.击退力度 = 铁质长剑_击退力度;
        数据.体力消耗 = 铁质长剑_体力消耗;
        数据.体力恢复 = 铁质长剑_体力恢复;
        数据.碰撞器尺寸 = 铁质长剑_碰撞器尺寸;
        return 数据;
    }

    /// <summary>
    /// 创建黄金巨剑的武器数据
    /// </summary>
    武器数据 创建黄金巨剑数据()
    {
        武器数据 数据 = new 武器数据("黄金巨剑", 黄金巨剑Sprite);
        数据.旋转速度 = 黄金巨剑_旋转速度;
        数据.最大蓄力时间 = 黄金巨剑_最大蓄力时间;
        数据.最小冲刺力度 = 黄金巨剑_最小冲刺力度;
        数据.最大冲刺力度 = 黄金巨剑_最大冲刺力度;
        数据.基础伤害 = 黄金巨剑_基础伤害;
        数据.蓄力加成伤害 = 黄金巨剑_蓄力加成伤害;
        数据.击退力度 = 黄金巨剑_击退力度;
        数据.体力消耗 = 黄金巨剑_体力消耗;
        数据.体力恢复 = 黄金巨剑_体力恢复;
        数据.碰撞器尺寸 = 黄金巨剑_碰撞器尺寸;
        return 数据;
    }

    /// <summary>
    /// 选择武器
    /// </summary>
    void 选择武器(武器数据 weaponData)
    {
        if (weaponData == null || weaponData.武器Sprite == null)
        {
            Debug.LogWarning($"武器数据无效: {weaponData?.武器名称 ?? "未知"}");
            return;
        }

        当前选择的武器数据 = weaponData;

        // 保存到管理器
        武器选择管理器.设置选择的武器(weaponData);

        // 启用开始按钮
        if (开始游戏按钮 != null)
        {
            开始游戏按钮.interactable = true;
        }

        Debug.Log($"选择了武器: {weaponData.武器名称}");
    }

    /// <summary>
    /// 开始游戏：切换到战斗场景
    /// </summary>
    void 开始游戏()
    {
        if (当前选择的武器数据 == null)
        {
            Debug.LogWarning("请先选择武器！");
            return;
        }

        // 加载战斗场景
        SceneManager.LoadScene(游戏战斗场景名称);
    }
}