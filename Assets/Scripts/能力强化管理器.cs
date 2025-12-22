using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class 能力强化管理器 : MonoBehaviour
{
    [Header("显示区域引用")]
    public Image 属性图标显示;
    public TextMeshProUGUI 属性名称文本;
    public TextMeshProUGUI 属性数值文本; // 显示 "当前: 100 -> 下一级: 110"
    public TextMeshProUGUI 属性介绍文本;

    [Header("强化操作引用")]
    public Button 强化按钮;
    public TextMeshProUGUI 强化花费文本;

    [Header("属性选择按钮")]
    public Button 生命值选择按钮;
    public Button 护甲值选择按钮;
    public Button 体力值选择按钮;
    public Button 拾取范围选择按钮;

    [Header("属性图标资源")]
    public Sprite 生命值Sprite;
    public Sprite 护甲值Sprite;
    public Sprite 体力值Sprite;
    public Sprite 拾取范围Sprite;

    [Header("强化数值配置")]
    public float 生命值每次增加量 = 2f; // 每级加2点血上限
    public float 护甲值每次增加量 = 1f;   // 每级加1点护甲上限
    public float 体力值每次增加量 = 10f;  // 每级加10点体力
    public float 拾取范围增加量 = 0.5f; // 每级加0.5范围

    [Header("强化花费配置")]
    // 0级升1级花费10，1级升2级花费20，以此类推
    public int[] 强化价格表 = new int[] { 10, 20, 30, 40, 50 };
    private const int 最大强化等级 = 5;

    // 内部常量 Key，用于存取 PlayerPrefs
    private const string 生命值等级KEY = "骑士冲锋生命值等级";
    private const string 护甲值等级KEY = "骑士冲锋护甲值等级";
    private const string 体力值等级KEY = "骑士冲锋体力值等级";
    private const string 拾取范围等级KEY = "骑士冲锋拾取范围等级";

    // 当前选中的属性类型（用于判断强化按钮点击后该升级谁）
    private string 当前选择的属性类型 = "";
    // 常量类型标识
    private const string TYPE生命值 = "生命值";
    private const string TYPE护甲值 = "护甲值";
    private const string TYPE体力值 = "体力值";
    private const string TYPE拾取范围 = "拾取范围";
    void Start()
    {
        // 1. 初始化属性 (非常重要！确保游戏开始时，玩家身上的属性是加上了强化值的)
        初始化玩家属性数值();

        // 2. 绑定选择按钮事件
        if (生命值选择按钮 != null) 生命值选择按钮.onClick.AddListener(选择生命值);
        if (护甲值选择按钮 != null) 护甲值选择按钮.onClick.AddListener(选择护甲值);
        if (体力值选择按钮 != null) 体力值选择按钮.onClick.AddListener(选择体力值);
        if (拾取范围选择按钮 != null) 拾取范围选择按钮.onClick.AddListener(选择拾取范围);

        // 3. 绑定强化按钮事件
        if (强化按钮 != null) 强化按钮.onClick.AddListener(执行强化);

        // 4. 默认选中第一项
        选择生命值();
    }
    void 初始化玩家属性数值()
    {
        if (玩家属性.Instance == null) return;

        // 恢复生命值
        int 生命值等级 = PlayerPrefs.GetInt(生命值等级KEY, 0);
        玩家属性.Instance.最大生命值 += (int)(生命值等级 * 生命值每次增加量);

        // 恢复护甲
        int 护甲值等级 = PlayerPrefs.GetInt(护甲值等级KEY, 0);
        玩家属性.Instance.最大护甲值 += (int)(护甲值等级 * 护甲值每次增加量);

        // 恢复体力
        int 体力值等级 = PlayerPrefs.GetInt(体力值等级KEY, 0);
        玩家属性.Instance.最大体力值 += (体力值等级 * 体力值每次增加量);

        // 恢复拾取范围
        int 拾取范围等级 = PlayerPrefs.GetInt(拾取范围等级KEY, 0);
        玩家属性.Instance.拾取范围 += (拾取范围等级 * 拾取范围增加量);
    }

    void 选择生命值()
    {
        当前选择的属性类型 = TYPE生命值;
        更新UI显示("最大生命值", "增加角色的最大生命值上限。", 生命值Sprite,
            生命值等级KEY, 玩家属性.Instance.最大生命值, 生命值每次增加量);
    }

    void 选择护甲值()
    {
        当前选择的属性类型 = TYPE护甲值;
        更新UI显示("最大护甲值", "增加角色的护甲值上限，提供更好的防护。", 护甲值Sprite,
            护甲值等级KEY, 玩家属性.Instance.最大护甲值, 护甲值每次增加量);
    }

    void 选择体力值()
    {
        当前选择的属性类型 = TYPE体力值;
        更新UI显示("最大体力", "增加冲锋所需的体力上限。", 体力值Sprite,
            体力值等级KEY, 玩家属性.Instance.最大体力值, 体力值每次增加量);
    }

    void 选择拾取范围()
    {
        当前选择的属性类型 = TYPE拾取范围;
        更新UI显示("拾取范围", "增加自动吸附金币和道具的范围。", 拾取范围Sprite,
            拾取范围等级KEY, 玩家属性.Instance.拾取范围, 拾取范围增加量);
    }
    void 更新UI显示(string name, string desc, Sprite sprite, string saveKey, float currentVal, float increaseAmount)
    {
        // 1. 基础信息
        属性图标显示.sprite = sprite;
        属性名称文本.text = name;
        属性介绍文本.text = desc;

        // 2. 获取等级和花费
        int currentLevel = PlayerPrefs.GetInt(saveKey, 0);

        // 3. 处理满级和数值显示
        if (currentLevel >= 最大强化等级)
        {
            属性数值文本.text = $"当前: {currentVal} (已满级)";
            强化花费文本.text = "MAX";
            强化按钮.interactable = false; // 禁用按钮
        }
        else
        {
            float nextVal = currentVal + increaseAmount;
            属性数值文本.text = $"当前: {currentVal} →{nextVal}";

            // 获取当前等级对应的价格
            int cost = 获取当前花费(currentLevel);
            强化花费文本.text = cost.ToString();
            强化按钮.interactable = true; // 启用按钮
        }
    }

    int 获取当前花费(int level)
    {
        if (level >= 强化价格表.Length) return 99999;
        return 强化价格表[level];
    }
    void 执行强化()
    {
        if (string.IsNullOrEmpty(当前选择的属性类型)) return;

        // 1. 根据当前类型确定 Key 和 参数
        string key = "";
        float increaseAmt = 0;

        // 用于重新刷新UI
        System.Action refreshAction = null;

        switch (当前选择的属性类型)
        {
            case TYPE生命值:
                key = 生命值等级KEY;
                increaseAmt = 生命值每次增加量;
                refreshAction = 选择生命值;
                break;
            case TYPE护甲值:
                key = 护甲值等级KEY;
                increaseAmt = 护甲值每次增加量;
                refreshAction = 选择护甲值;
                break;
            case TYPE体力值:
                key = 体力值等级KEY;
                increaseAmt = 体力值每次增加量;
                refreshAction = 选择体力值;
                break;
            case TYPE拾取范围:
                key = 拾取范围等级KEY;
                increaseAmt = 拾取范围增加量;
                refreshAction = 选择拾取范围;
                break;
        }

        // 2. 检查等级
        int currentLevel = PlayerPrefs.GetInt(key, 0);
        if (currentLevel >= 最大强化等级) return;

        // 3. 检查金币并扣除
        int cost = 获取当前花费(currentLevel);
        if (玩家属性.Instance.扣除金币(cost))
        {
            // 4. 实际修改玩家属性 (直接修改 Instance)
            switch (当前选择的属性类型)
            {
                case TYPE生命值: 玩家属性.Instance.最大生命值 += (int)increaseAmt; break;
                case TYPE护甲值: 玩家属性.Instance.最大护甲值 += (int)increaseAmt; break;
                case TYPE体力值: 玩家属性.Instance.最大体力值 += increaseAmt; break;
                case TYPE拾取范围: 玩家属性.Instance.拾取范围 += increaseAmt; break;
            }

            // 5. 保存等级
            PlayerPrefs.SetInt(key, currentLevel + 1);
            PlayerPrefs.Save();

            // 6. 刷新界面
            if (refreshAction != null) refreshAction.Invoke();

            Debug.Log($"强化属性 {当前选择的属性类型} 成功！等级 {currentLevel} -> {currentLevel + 1}");
        }
        else
        {
            Debug.Log("金币不足，无法强化属性！");
            // 这里可以加一个金币不足的弹窗或动画
        }
    }
}
