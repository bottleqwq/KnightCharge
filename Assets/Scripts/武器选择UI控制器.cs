using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

/// <summary>
/// 武器选择UI控制器：处理武器选择界面的交互
/// </summary>
public class 武器选择UI控制器 : MonoBehaviour
{
    [Header("UI引用")]
    public Image 武器图标;
    public TextMeshProUGUI 武器名称;
    public TextMeshProUGUI 旋转速度;
    public TextMeshProUGUI 基础伤害;
    public TextMeshProUGUI 蓄力加成伤害;
    public TextMeshProUGUI 最大蓄力时间;
    public TextMeshProUGUI 最小冲刺力度;
    public TextMeshProUGUI 最大冲刺力度;
    public TextMeshProUGUI 击退力度;
    public TextMeshProUGUI 体力消耗;
    public TextMeshProUGUI 武器介绍;

    [Header("强化系统")]
    public Button 强化基础伤害按钮;
    public Button 强化蓄力加成伤害按钮;
    public Button 强化体力消耗按钮;
    public float 基础伤害强化值 = 5f;
    public float 蓄力加成伤害强化值 = 5f;
    public float 体力消耗强化值 = 1f;

    [Header("武器选择按钮设置")]
    public Button 新手木剑选择按钮;
    public Button 大剑选择按钮;
    public Button 细剑选择按钮;
    public Button 长矛选择按钮;
    public Button 锤子选择按钮;
    public Button 镰刀选择按钮;

    [Header("武器Sprite引用")]
    public Sprite 新手木剑Sprite;
    public Sprite 大剑Sprite;
    public Sprite 细剑Sprite;
    public Sprite 长矛Sprite;
    public Sprite 锤子Sprite;
    public Sprite 镰刀Sprite;

    [Header("新手木剑属性")]
    public float 新手木剑_旋转速度 = 250f;
    public float 新手木剑_最大蓄力时间 = 2.5f;
    public float 新手木剑_最小冲刺力度 = 0.8f;
    public float 新手木剑_最大冲刺力度 = 8f;
    public float 新手木剑_基础伤害 = 8f;
    public float 新手木剑_蓄力加成伤害 = 15f;
    public float 新手木剑_击退力度 = 8f;
    public float 新手木剑_体力消耗 = 8f;
    public Vector2 新手木剑_碰撞器尺寸 = new Vector2(0.3f, 0.8f);
    [TextArea]
    public string 新手木剑_武器介绍;

    [Header("大剑属性")]
    public float 大剑_旋转速度 = 320f;
    public float 大剑_最大蓄力时间 = 2.0f;
    public float 大剑_最小冲刺力度 = 1.0f;
    public float 大剑_最大冲刺力度 = 10f;
    public float 大剑_基础伤害 = 12f;
    public float 大剑_蓄力加成伤害 = 20f;
    public float 大剑_击退力度 = 12f;
    public float 大剑_体力消耗 = 10f;
    public Vector2 大剑_碰撞器尺寸 = new Vector2(0.2f, 0.64f);
    [TextArea]
    public string 大剑_武器介绍;

    [Header("细剑属性")]
    public float 细剑_旋转速度 = 320f;
    public float 细剑_最大蓄力时间 = 2.0f;
    public float 细剑_最小冲刺力度 = 1.0f;
    public float 细剑_最大冲刺力度 = 10f;
    public float 细剑_基础伤害 = 12f;
    public float 细剑_蓄力加成伤害 = 20f;
    public float 细剑_击退力度 = 12f;
    public float 细剑_体力消耗 = 10f;
    public Vector2 细剑_碰撞器尺寸 = new Vector2(0.2f, 0.64f);
    [TextArea]
    public string 细剑_武器介绍;

    [Header("长矛属性")]
    public float 长矛_旋转速度 = 320f;
    public float 长矛_最大蓄力时间 = 2.0f;
    public float 长矛_最小冲刺力度 = 1.0f;
    public float 长矛_最大冲刺力度 = 10f;
    public float 长矛_基础伤害 = 12f;
    public float 长矛_蓄力加成伤害 = 20f;
    public float 长矛_击退力度 = 12f;
    public float 长矛_体力消耗 = 10f;
    public Vector2 长矛_碰撞器尺寸 = new Vector2(0.2f, 0.64f);
    [TextArea]
    public string 长矛_武器介绍;

    [Header("锤子属性")]
    public float 锤子_旋转速度 = 320f;
    public float 锤子_最大蓄力时间 = 2.0f;
    public float 锤子_最小冲刺力度 = 1.0f;
    public float 锤子_最大冲刺力度 = 10f;
    public float 锤子_基础伤害 = 12f;
    public float 锤子_蓄力加成伤害 = 20f;
    public float 锤子_击退力度 = 12f;
    public float 锤子_体力消耗 = 10f;
    public Vector2 锤子_碰撞器尺寸 = new Vector2(0.2f, 0.64f);
    [TextArea]
    public string 锤子_武器介绍;

    [Header("镰刀属性")]
    public float 镰刀_旋转速度 = 320f;
    public float 镰刀_最大蓄力时间 = 2.0f;
    public float 镰刀_最小冲刺力度 = 1.0f;
    public float 镰刀_最大冲刺力度 = 10f;
    public float 镰刀_基础伤害 = 12f;
    public float 镰刀_蓄力加成伤害 = 20f;
    public float 镰刀_击退力度 = 12f;
    public float 镰刀_体力消耗 = 10f;
    public Vector2 镰刀_碰撞器尺寸 = new Vector2(0.2f, 0.64f);
    [TextArea]
    public string 镰刀_武器介绍;

    [Header("UI设置")]
    public Button 开始游戏按钮;
    public string 游戏战斗场景名称 = "游戏战斗场景"; // 战斗场景的名称

    private 武器数据 当前选择的武器数据 = null;

    void Start()
    {
        // 绑定武器选择按钮
        if (新手木剑选择按钮 != null)
        {
            新手木剑选择按钮.onClick.AddListener(() => 选择武器(创建新手木剑数据()));
        }
        if (大剑选择按钮 != null)
        {
            大剑选择按钮.onClick.AddListener(() => 选择武器(创建大剑数据()));
        }
        if (细剑选择按钮 != null)
        {
            细剑选择按钮.onClick.AddListener(() => 选择武器(创建细剑数据()));
        }
        if (长矛选择按钮 != null)
        {
            长矛选择按钮.onClick.AddListener(() => 选择武器(创建长矛数据()));
        }
        if (锤子选择按钮 != null)
        {
            锤子选择按钮.onClick.AddListener(() => 选择武器(创建锤子数据()));
        }
        if (镰刀选择按钮 != null)
        {
            镰刀选择按钮.onClick.AddListener(() => 选择武器(创建镰刀数据()));
        }
        // 绑定开始游戏按钮
        if (开始游戏按钮 != null)
        {
            开始游戏按钮.onClick.AddListener(开始游戏);
        }

        if (强化基础伤害按钮 != null)
        {
            强化基础伤害按钮.onClick.AddListener(强化基础伤害);
            // 初始状态下，未选择武器时禁用强化按钮
            强化基础伤害按钮.interactable = false;
        }

        // 初始状态：开始按钮可能应该禁用，直到选择了武器
        if (开始游戏按钮 != null)
        {
            开始游戏按钮.interactable = false;
        }

        选择武器(创建新手木剑数据());
       
        if (当前选择的武器数据.武器名称 == "新手木剑")
        {
            强化基础伤害按钮.gameObject.SetActive(false);
            强化蓄力加成伤害按钮.gameObject.SetActive(false);
            强化体力消耗按钮.gameObject.SetActive(false);
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
        数据.碰撞器尺寸 = 新手木剑_碰撞器尺寸;
        数据.武器介绍 = 新手木剑_武器介绍;
        return 数据;
    }

    /// <summary>
    /// 创建大剑的武器数据
    /// </summary>
    武器数据 创建大剑数据()
    {
        武器数据 数据 = new 武器数据("大剑", 大剑Sprite);
        数据.旋转速度 = 大剑_旋转速度;
        数据.最大蓄力时间 = 大剑_最大蓄力时间;
        数据.最小冲刺力度 = 大剑_最小冲刺力度;
        数据.最大冲刺力度 = 大剑_最大冲刺力度;
        数据.基础伤害 = 大剑_基础伤害;
        数据.蓄力加成伤害 = 大剑_蓄力加成伤害;
        数据.击退力度 = 大剑_击退力度;
        数据.体力消耗 = 大剑_体力消耗;
        数据.碰撞器尺寸 = 大剑_碰撞器尺寸;
        数据.武器介绍 = 大剑_武器介绍;
        return 数据;
    }

    /// <summary>
    /// 创建细剑的武器数据
    /// </summary>
    武器数据 创建细剑数据()
    {
        武器数据 数据 = new 武器数据("细剑", 细剑Sprite);
        数据.旋转速度 = 细剑_旋转速度;
        数据.最大蓄力时间 = 细剑_最大蓄力时间;
        数据.最小冲刺力度 = 细剑_最小冲刺力度;
        数据.最大冲刺力度 = 细剑_最大冲刺力度;
        数据.基础伤害 = 细剑_基础伤害;
        数据.蓄力加成伤害 = 细剑_蓄力加成伤害;
        数据.击退力度 = 细剑_击退力度;
        数据.体力消耗 = 细剑_体力消耗;
        数据.碰撞器尺寸 = 细剑_碰撞器尺寸;
        数据.武器介绍 = 细剑_武器介绍;
        return 数据;
    }

    /// <summary>
    /// 创建细剑的武器数据
    /// </summary>
    武器数据 创建长矛数据()
    {
        武器数据 数据 = new 武器数据("长矛", 长矛Sprite);
        数据.旋转速度 = 长矛_旋转速度;
        数据.最大蓄力时间 = 长矛_最大蓄力时间;
        数据.最小冲刺力度 = 长矛_最小冲刺力度;
        数据.最大冲刺力度 = 长矛_最大冲刺力度;
        数据.基础伤害 = 长矛_基础伤害;
        数据.蓄力加成伤害 = 长矛_蓄力加成伤害;
        数据.击退力度 = 长矛_击退力度;
        数据.体力消耗 = 长矛_体力消耗;
        数据.碰撞器尺寸 = 长矛_碰撞器尺寸;
        数据.武器介绍 = 长矛_武器介绍;
        return 数据;
    }

    /// <summary>
    /// 创建细剑的武器数据
    /// </summary>
    武器数据 创建锤子数据()
    {
        武器数据 数据 = new 武器数据("锤子", 锤子Sprite);
        数据.旋转速度 = 锤子_旋转速度;
        数据.最大蓄力时间 = 锤子_最大蓄力时间;
        数据.最小冲刺力度 = 锤子_最小冲刺力度;
        数据.最大冲刺力度 = 锤子_最大冲刺力度;
        数据.基础伤害 = 锤子_基础伤害;
        数据.蓄力加成伤害 = 锤子_蓄力加成伤害;
        数据.击退力度 = 锤子_击退力度;
        数据.体力消耗 = 锤子_体力消耗;
        数据.碰撞器尺寸 = 锤子_碰撞器尺寸;
        数据.武器介绍 = 锤子_武器介绍;
        return 数据;
    }

    武器数据 创建镰刀数据()
    {
        武器数据 数据 = new 武器数据("镰刀", 镰刀Sprite);
        数据.旋转速度 = 镰刀_旋转速度;
        数据.最大蓄力时间 = 镰刀_最大蓄力时间;
        数据.最小冲刺力度 = 镰刀_最小冲刺力度;
        数据.最大冲刺力度 = 镰刀_最大冲刺力度;
        数据.基础伤害 = 镰刀_基础伤害;
        数据.蓄力加成伤害 = 镰刀_蓄力加成伤害;
        数据.击退力度 = 镰刀_击退力度;
        数据.体力消耗 = 镰刀_体力消耗;
        数据.碰撞器尺寸 = 镰刀_碰撞器尺寸;
        数据.武器介绍 = 镰刀_武器介绍;
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

        武器图标.sprite = weaponData.武器Sprite;        
        武器名称.text = $"武器名称：{weaponData.武器名称}";
        旋转速度.text = $"旋转速度：{weaponData.旋转速度}";
        基础伤害.text = $"基础伤害：{weaponData.基础伤害}";
        蓄力加成伤害.text = $"蓄力加成伤害：{weaponData.蓄力加成伤害}";
        最大蓄力时间.text = $"最大蓄力时间：{weaponData.最大蓄力时间}";
        最小冲刺力度.text = $"最小冲刺力度：{weaponData.最小冲刺力度}";
        最大冲刺力度.text = $"最大冲刺力度：{weaponData.最大冲刺力度}";
        击退力度.text = $"击退力度：{weaponData.击退力度}";
        体力消耗.text = $"体力消耗：{weaponData.体力消耗}";
        武器介绍.text = $"武器介绍：{weaponData.武器介绍}";
        当前选择的武器数据 = weaponData;

        if (当前选择的武器数据.武器名称 == "新手木剑")
        {
            强化基础伤害按钮.gameObject.SetActive(false);
            强化蓄力加成伤害按钮.gameObject.SetActive(false);
            强化体力消耗按钮.gameObject.SetActive(false);
        }
        else
        {
            强化基础伤害按钮.gameObject.SetActive(true);
            强化蓄力加成伤害按钮.gameObject.SetActive(true);
            强化体力消耗按钮.gameObject.SetActive(true);
        }
        // 保存到管理器
        武器选择管理器.设置选择的武器(weaponData);

        // 启用开始按钮
        if (开始游戏按钮 != null)
        {
            开始游戏按钮.interactable = true;
        }

        if (强化基础伤害按钮 != null)
        {
            强化基础伤害按钮.interactable = true;
        }

        Debug.Log($"选择了武器: {weaponData.武器名称}");
    }
    /// <summary>
    /// 点击强化按钮时调用
    /// </summary>
    void 强化基础伤害()
    {
        if (当前选择的武器数据 == null) return;

        string currentName = 当前选择的武器数据.武器名称;

        // 1. 根据名字修改对应的“源变量”
        // 这样做的目的是确保修改只针对特定武器，且切换回来后数据依然是保留的
        switch (currentName)
        {
            case "新手木剑":
                新手木剑_基础伤害 += 基础伤害强化值;
                // 这里调用 选择武器 重新刷新界面
                选择武器(创建新手木剑数据());
                break;

            case "大剑":
                大剑_基础伤害 += 基础伤害强化值;
                选择武器(创建大剑数据());
                break;

            case "细剑":
                细剑_基础伤害 += 基础伤害强化值;
                选择武器(创建细剑数据());
                break;

            case "长矛":
                长矛_基础伤害 += 基础伤害强化值;
                选择武器(创建长矛数据());
                break;

            case "锤子":
                锤子_基础伤害 += 基础伤害强化值;
                选择武器(创建锤子数据());
                break;

            case "镰刀":
                镰刀_基础伤害 += 基础伤害强化值;
                选择武器(创建镰刀数据());
                break;

            default:
                Debug.LogWarning("未知的武器名称，无法强化");
                break;
        }

        Debug.Log($"已强化 {currentName}，基础伤害增加了 {基础伤害强化值}");
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