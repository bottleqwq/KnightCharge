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

    [Header("强化系统 UI")]
    public Button 强化基础伤害按钮;
    public Button 强化蓄力加成伤害按钮;
    public Button 强化体力消耗按钮;

    // 分别显示花费的文本
    public TextMeshProUGUI 基础伤害强化花费;
    public TextMeshProUGUI 蓄力伤害强化花费; // 建议你在Unity里也把这个绑定上
    public TextMeshProUGUI 体力消耗强化花费; // 建议你在Unity里也把这个绑定上

    [Header("强化数值配置")]
    public float 基础伤害强化值 = 5f;
    public float 蓄力加成伤害强化值 = 5f;
    public float 体力消耗强化值 = 1f; // 注意：体力消耗通常是减少，这里假设是数值优化

    // 定义最大等级
    private const int 最大强化等级 = 5;

    // 定义每一级的花费 (下标0代表从0级升到1级的花费，下标4代表4级升到5级)
    private int[] 强化价格表 = new int[] { 1, 2, 3, 4, 5 };

    // 定义属性类型的常量Key，防止写错字符串
    private const string TYPE_BASE_DMG = "基础伤害";
    private const string TYPE_CHARGE_DMG = "蓄力伤害";
    private const string TYPE_STAMINA = "体力消耗";

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
            开始游戏按钮.interactable = false;
        }

        if (强化基础伤害按钮 != null)
        {
            强化基础伤害按钮.onClick.AddListener(强化基础伤害);
        }
        if (强化蓄力加成伤害按钮 != null)
        {
            强化蓄力加成伤害按钮.onClick.AddListener(强化蓄力伤害);
        }
        if (强化体力消耗按钮 != null)
        {
            强化体力消耗按钮.onClick.AddListener(强化体力消耗);
        }

        初始化所有武器数值();
        // 默认选择新手木剑
        选择武器(创建新手木剑数据());
    }

    武器数据 创建新手木剑数据()
    { return new 武器数据("新手木剑", 新手木剑Sprite) { 旋转速度 = 新手木剑_旋转速度, 基础伤害 = 新手木剑_基础伤害, 蓄力加成伤害 = 新手木剑_蓄力加成伤害, 最大蓄力时间 = 新手木剑_最大蓄力时间, 最小冲刺力度 = 新手木剑_最小冲刺力度, 最大冲刺力度 = 新手木剑_最大冲刺力度, 击退力度 = 新手木剑_击退力度, 体力消耗 = 新手木剑_体力消耗, 碰撞器尺寸 = 新手木剑_碰撞器尺寸, 武器介绍 = 新手木剑_武器介绍 }; }
 
    武器数据 创建大剑数据()
    { return new 武器数据("大剑", 大剑Sprite) { 旋转速度 = 大剑_旋转速度, 基础伤害 = 大剑_基础伤害, 蓄力加成伤害 = 大剑_蓄力加成伤害, 最大蓄力时间 = 大剑_最大蓄力时间, 最小冲刺力度 = 大剑_最小冲刺力度, 最大冲刺力度 = 大剑_最大冲刺力度, 击退力度 = 大剑_击退力度, 体力消耗 = 大剑_体力消耗, 碰撞器尺寸 = 大剑_碰撞器尺寸, 武器介绍 = 大剑_武器介绍 }; }

    武器数据 创建细剑数据()
    { return new 武器数据("细剑", 细剑Sprite) { 旋转速度 = 细剑_旋转速度, 基础伤害 = 细剑_基础伤害, 蓄力加成伤害 = 细剑_蓄力加成伤害, 最大蓄力时间 = 细剑_最大蓄力时间, 最小冲刺力度 = 细剑_最小冲刺力度, 最大冲刺力度 = 细剑_最大冲刺力度, 击退力度 = 细剑_击退力度, 体力消耗 = 细剑_体力消耗, 碰撞器尺寸 = 细剑_碰撞器尺寸, 武器介绍 = 细剑_武器介绍 }; }

    武器数据 创建长矛数据()
    { return new 武器数据("长矛", 长矛Sprite) { 旋转速度 = 长矛_旋转速度, 基础伤害 = 长矛_基础伤害, 蓄力加成伤害 = 长矛_蓄力加成伤害, 最大蓄力时间 = 长矛_最大蓄力时间, 最小冲刺力度 = 长矛_最小冲刺力度, 最大冲刺力度 = 长矛_最大冲刺力度, 击退力度 = 长矛_击退力度, 体力消耗 = 长矛_体力消耗, 碰撞器尺寸 = 长矛_碰撞器尺寸, 武器介绍 = 长矛_武器介绍 }; }

    武器数据 创建锤子数据()
    { return new 武器数据("锤子", 锤子Sprite) { 旋转速度 = 锤子_旋转速度, 基础伤害 = 锤子_基础伤害, 蓄力加成伤害 = 锤子_蓄力加成伤害, 最大蓄力时间 = 锤子_最大蓄力时间, 最小冲刺力度 = 锤子_最小冲刺力度, 最大冲刺力度 = 锤子_最大冲刺力度, 击退力度 = 锤子_击退力度, 体力消耗 = 锤子_体力消耗, 碰撞器尺寸 = 锤子_碰撞器尺寸, 武器介绍 = 锤子_武器介绍 }; }

    武器数据 创建镰刀数据()
    { return new 武器数据("镰刀", 镰刀Sprite) { 旋转速度 = 镰刀_旋转速度, 基础伤害 = 镰刀_基础伤害, 蓄力加成伤害 = 镰刀_蓄力加成伤害, 最大蓄力时间 = 镰刀_最大蓄力时间, 最小冲刺力度 = 镰刀_最小冲刺力度, 最大冲刺力度 = 镰刀_最大冲刺力度, 击退力度 = 镰刀_击退力度, 体力消耗 = 镰刀_体力消耗, 碰撞器尺寸 = 镰刀_碰撞器尺寸, 武器介绍 = 镰刀_武器介绍 }; }

    /// <summary>
    /// 获取指定武器、指定属性的当前强化等级
    /// Key的格式示例: "大剑_基础伤害_Level"
    /// </summary>
    private int 获取武器属性等级(string weaponName, string statType)
    {
        string key = $"{weaponName}_{statType}_Level";
        return PlayerPrefs.GetInt(key, 0); // 默认为0级
    }

    /// <summary>
    /// 增加等级并保存
    /// </summary>
    private void 增加武器属性等级(string weaponName, string statType)
    {
        string key = $"{weaponName}_{statType}_Level";
        int currentLevel = 获取武器属性等级(weaponName, statType);
        PlayerPrefs.SetInt(key, currentLevel + 1);
        PlayerPrefs.Save();
    }

    /// <summary>
    /// 根据当前等级获取下一次强化的花费
    /// </summary>
    private int 获取当前花费(int currentLevel)
    {
        if (currentLevel >= 强化价格表.Length)
            return 99999; // 超过上限（理论上UI会隐藏，这里做个保险）

        return 强化价格表[currentLevel];
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

        武器选择管理器.设置选择的武器(weaponData);


        // 启用开始按钮
        if (开始游戏按钮 != null)开始游戏按钮.interactable = true;        
        
        刷新强化UI状态();
    }

    /// <summary>
    /// 专门负责检查每个按钮是否应该显示，以及更新花费文字
    /// </summary>
    void 刷新强化UI状态()
    {
        string weaponName = 当前选择的武器数据.武器名称;

        // 1. 如果是新手木剑，全部隐藏
        if (weaponName == "新手木剑")
        {
            设置按钮状态(强化基础伤害按钮, false);
            设置按钮状态(强化蓄力加成伤害按钮, false);
            设置按钮状态(强化体力消耗按钮, false);
            return;
        }

        // 2. 检查 基础伤害 按钮
        int dmgLevel = 获取武器属性等级(weaponName, TYPE_BASE_DMG);
        if (dmgLevel >= 最大强化等级)
        {
            设置按钮状态(强化基础伤害按钮, false); // 满级隐藏
            if (基础伤害强化花费 != null) 基础伤害强化花费.text = "MAX";
        }
        else
        {
            设置按钮状态(强化基础伤害按钮, true);
            if (基础伤害强化花费 != null) 基础伤害强化花费.text = 获取当前花费(dmgLevel).ToString();
        }

        // 3. 检查 蓄力伤害 按钮 (逻辑同上，彼此独立)
        int chargeLevel = 获取武器属性等级(weaponName, TYPE_CHARGE_DMG);
        if (chargeLevel >= 最大强化等级)
        {
            设置按钮状态(强化蓄力加成伤害按钮, false);
            if (蓄力伤害强化花费 != null) 蓄力伤害强化花费.text = "MAX";
        }
        else
        {
            设置按钮状态(强化蓄力加成伤害按钮, true);
            if (蓄力伤害强化花费 != null) 蓄力伤害强化花费.text = 获取当前花费(chargeLevel).ToString();
        }

        // 4. 检查 体力消耗 按钮
        int staminaLevel = 获取武器属性等级(weaponName, TYPE_STAMINA);
        if (staminaLevel >= 最大强化等级)
        {
            设置按钮状态(强化体力消耗按钮, false);
            if (体力消耗强化花费 != null) 体力消耗强化花费.text = "MAX";
        }
        else
        {
            设置按钮状态(强化体力消耗按钮, true);
            if (体力消耗强化花费 != null) 体力消耗强化花费.text = 获取当前花费(staminaLevel).ToString();
        }
    }
    void 设置按钮状态(Button btn, bool isActive)
    {
        if (btn != null) btn.gameObject.SetActive(isActive);
    }

    void 强化基础伤害()
    {
        if (当前选择的武器数据 == null) return;
        string currentWeapon = 当前选择的武器数据.武器名称;

        // 1. 获取当前这把武器的基础伤害等级
        int currentLevel = 获取武器属性等级(currentWeapon, TYPE_BASE_DMG);

        // 2. 检查是否满级 (双重保险)
        if (currentLevel >= 最大强化等级) return;

        // 3. 计算花费
        int cost = 获取当前花费(currentLevel);

        // 4. 扣钱
        if (玩家属性.Instance.扣除金币(cost))
        {
            // 5. 实际修改属性数值
            switch (currentWeapon)
            {
                case "大剑": 大剑_基础伤害 += 基础伤害强化值; break;
                case "细剑": 细剑_基础伤害 += 基础伤害强化值; break;
                case "长矛": 长矛_基础伤害 += 基础伤害强化值; break;
                case "锤子": 锤子_基础伤害 += 基础伤害强化值; break;
                case "镰刀": 镰刀_基础伤害 += 基础伤害强化值; break;
            }

            // 6. 记录等级提升
            增加武器属性等级(currentWeapon, TYPE_BASE_DMG);

            // 7. 刷新界面 (重新生成数据以更新显示的数值，并刷新按钮状态)
            重新加载当前武器();

            Debug.Log($"强化成功！{currentWeapon} 基础伤害等级升至 {currentLevel + 1}");
        }
        else
        {
            Debug.Log("金币不足！");
        }
    }
    void 强化蓄力伤害()
    {
        if (当前选择的武器数据 == null) return;
        string currentWeapon = 当前选择的武器数据.武器名称;

        // 1. 获取当前等级
        int currentLevel = 获取武器属性等级(currentWeapon, TYPE_CHARGE_DMG);

        // 2. 检查是否满级
        if (currentLevel >= 最大强化等级) return;

        // 3. 计算花费
        int cost = 获取当前花费(currentLevel);

        // 4. 扣钱并强化
        if (玩家属性.Instance.扣除金币(cost))
        {
            // 修改对应的变量
            switch (currentWeapon)
            {
                case "大剑": 大剑_蓄力加成伤害 += 蓄力加成伤害强化值; break;
                case "细剑": 细剑_蓄力加成伤害 += 蓄力加成伤害强化值; break;
                case "长矛": 长矛_蓄力加成伤害 += 蓄力加成伤害强化值; break;
                case "锤子": 锤子_蓄力加成伤害 += 蓄力加成伤害强化值; break;
                case "镰刀": 镰刀_蓄力加成伤害 += 蓄力加成伤害强化值; break;
            }

            // 记录等级提升
            增加武器属性等级(currentWeapon, TYPE_CHARGE_DMG);

            // 刷新界面
            重新加载当前武器();
            Debug.Log($"强化成功！{currentWeapon} 蓄力伤害等级升至 {currentLevel + 1}");
        }
        else
        {
            Debug.Log("金币不足！");
        }
    }
    void 强化体力消耗()
    {
        if (当前选择的武器数据 == null) return;
        string currentWeapon = 当前选择的武器数据.武器名称;

        int currentLevel = 获取武器属性等级(currentWeapon, TYPE_STAMINA);

        if (currentLevel >= 最大强化等级) return;

        int cost = 获取当前花费(currentLevel);

        if (玩家属性.Instance.扣除金币(cost))
        {
            // 修改对应的变量 (注意：体力消耗是越强化越低，所以用减法)
            switch (currentWeapon)
            {
                case "大剑": 大剑_体力消耗 -= 体力消耗强化值; break;
                case "细剑": 细剑_体力消耗 -= 体力消耗强化值; break;
                case "长矛": 长矛_体力消耗 -= 体力消耗强化值; break;
                case "锤子": 锤子_体力消耗 -= 体力消耗强化值; break;
                case "镰刀": 镰刀_体力消耗 -= 体力消耗强化值; break;
            }

            // 防止减成负数（可选保险）
            // if (大剑_体力消耗 < 0) 大剑_体力消耗 = 0; 

            增加武器属性等级(currentWeapon, TYPE_STAMINA);
            重新加载当前武器();
            Debug.Log($"强化成功！{currentWeapon} 体力消耗等级升至 {currentLevel + 1}");
        }
        else
        {
            Debug.Log("金币不足！");
        }
    }
    /// <summary>
    /// 辅助函数：根据当前名字重新调用选择武器，用于刷新界面
    /// </summary>
    void 重新加载当前武器()
    {
        string name = 当前选择的武器数据.武器名称;
        switch (name)
        {
            case "新手木剑": 选择武器(创建新手木剑数据()); break;
            case "大剑": 选择武器(创建大剑数据()); break;
            case "细剑": 选择武器(创建细剑数据()); break;
            case "长矛": 选择武器(创建长矛数据()); break;
            case "锤子": 选择武器(创建锤子数据()); break;
            case "镰刀": 选择武器(创建镰刀数据()); break;
        }
    }
    void 初始化所有武器数值()
    {
        // 武器列表（方便遍历，或者你可以手动写一个个if，这里为了简单直接手动列出）
        string[] weapons = new string[] { "大剑", "细剑", "长矛", "锤子", "镰刀" };

        foreach (var weaponName in weapons)
        {
            // --- 恢复基础伤害 ---
            int dmgLvl = 获取武器属性等级(weaponName, TYPE_BASE_DMG);
            float addedDmg = dmgLvl * 基础伤害强化值;

            if (addedDmg > 0)
            {
                switch (weaponName)
                {
                    case "大剑": 大剑_基础伤害 += addedDmg; break;
                    case "细剑": 细剑_基础伤害 += addedDmg; break;
                    case "长矛": 长矛_基础伤害 += addedDmg; break;
                    case "锤子": 锤子_基础伤害 += addedDmg; break;
                    case "镰刀": 镰刀_基础伤害 += addedDmg; break;
                }
            }

            // --- 恢复蓄力伤害 ---
            int chargeLvl = 获取武器属性等级(weaponName, TYPE_CHARGE_DMG);
            float addedCharge = chargeLvl * 蓄力加成伤害强化值;

            if (addedCharge > 0)
            {
                switch (weaponName)
                {
                    case "大剑": 大剑_蓄力加成伤害 += addedCharge; break;
                    case "细剑": 细剑_蓄力加成伤害 += addedCharge; break;
                    case "长矛": 长矛_蓄力加成伤害 += addedCharge; break;
                    case "锤子": 锤子_蓄力加成伤害 += addedCharge; break;
                    case "镰刀": 镰刀_蓄力加成伤害 += addedCharge; break;
                }
            }

            // --- 恢复体力消耗 (记得是减法) ---
            int staminaLvl = 获取武器属性等级(weaponName, TYPE_STAMINA);
            float reducedStamina = staminaLvl * 体力消耗强化值;

            if (reducedStamina > 0)
            {
                switch (weaponName)
                {
                    case "大剑": 大剑_体力消耗 -= reducedStamina; break;
                    case "细剑": 细剑_体力消耗 -= reducedStamina; break;
                    case "长矛": 长矛_体力消耗 -= reducedStamina; break;
                    case "锤子": 锤子_体力消耗 -= reducedStamina; break;
                    case "镰刀": 镰刀_体力消耗 -= reducedStamina; break;
                }
            }
        }
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