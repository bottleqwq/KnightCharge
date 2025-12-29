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
    public GameObject 显示界面;
    public GameObject 提示文字;

    [Header("强化操作引用")]
    public Button 强化按钮;
    public TextMeshProUGUI 强化花费文本;

    [Header("属性选择按钮")]
    public Button 生命值选择按钮;
    public Button 护甲值选择按钮;
    public Button 体力值选择按钮;
    public Button 护甲恢复延迟选择按钮;
    public Button 护甲恢复速度选择按钮;
    public Button 护盾时长选择按钮;
    public Button 体力值恢复速度选择按钮;
    public Button 护盾爆率选择按钮;
    public Button 血瓶爆率选择按钮;
    public Button 金币爆率选择按钮;
    public Button 祝福固定增长选择按钮;
    public Button 祝福击杀增长选择按钮;
    public Button 生命窃取率选择按钮;
    public Button 闪避率选择按钮;

    [Header("属性图标资源")]
    public Sprite 生命值Sprite;
    public Sprite 护甲值Sprite;
    public Sprite 体力值Sprite;
    public Sprite 护甲恢复延迟Sprite;
    public Sprite 护甲恢复速度Sprite;
    public Sprite 护盾时长Sprite;
    public Sprite 体力值恢复速度Sprite;
    public Sprite 护盾爆率Sprite;
    public Sprite 血瓶爆率Sprite;
    public Sprite 金币爆率Sprite;
    public Sprite 祝福固定增长Sprite;
    public Sprite 祝福击杀增长Sprite;
    public Sprite 生命窃取率Sprite;
    public Sprite 闪避率Sprite;

    [Header("强化数值配置")]
    public float 生命值每次增加量 = 2f;
    public float 护甲值每次增加量 = 1f;
    public float 体力值每次增加量 = 10f;
    public float 护甲恢复延迟每次增加量 = 1f;
    public float 护甲恢复速度每次增加量 = 1f;
    public float 护盾时长每次增加量 = 1f;
    public float 体力值恢复速度每次增加量 = 1f;
    public float 护盾爆率每次增加量 = 1f;
    public float 血瓶爆率每次增加量 = 1f;
    public float 金币爆率每次增加量 = 1f;
    public float 祝福固定增长每次增加量 = 1f;
    public float 祝福击杀增长每次增加量 = 1f;
    public float 生命窃取率每次增加量 = 0.5f;
    public float 闪避率每次增加量 = 1f;

    [Header("强化花费配置")]
    // 0级升1级花费10，1级升2级花费20，以此类推
    public int[] 强化价格表 = new int[] { 1, 2, 3, 4, 5 };
    private const int 最大强化等级 = 5;

    // 内部常量 Key，用于存取 PlayerPrefs
    private const string 生命值等级KEY = "骑士冲锋生命值等级";
    private const string 护甲值等级KEY = "骑士冲锋护甲值等级";
    private const string 体力值等级KEY = "骑士冲锋体力值等级";
    private const string 护甲恢复延迟等级KEY = "骑士冲锋护甲恢复延迟等级";
    private const string 护甲恢复速度等级KEY = "骑士冲锋护甲恢复速度等级";
    private const string 护盾时长等级KEY = "骑士冲锋护盾时长等级";
    private const string 体力值恢复速度等级KEY = "骑士冲锋体力值恢复速度等级";
    private const string 护盾爆率等级KEY = "骑士冲锋护盾爆率等级";
    private const string 血瓶爆率等级KEY = "骑士冲锋血瓶爆率等级";
    private const string 金币爆率等级KEY = "骑士冲锋金币爆率等级";
    private const string 祝福固定增长等级KEY = "骑士冲锋祝福固定增长等级";
    private const string 祝福击杀增长等级KEY = "骑士冲锋祝福击杀增长等级";
    private const string 生命窃取率等级KEY = "骑士冲锋生命窃取率等级";
    private const string 闪避率等级KEY = "骑士冲锋闪避率等级";

    // 当前选中的属性类型（用于判断强化按钮点击后该升级谁）
    private string 当前选择的属性类型 = "";
    // 常量类型标识
    private const string TYPE生命值 = "生命值";
    private const string TYPE护甲值 = "护甲值";
    private const string TYPE体力值 = "体力值";
    private const string TYPE护甲恢复延迟 = "护甲恢复延迟";
    private const string TYPE护甲恢复速度 = "护甲恢复速度";
    private const string TYPE护盾时长 = "护盾时长";
    private const string TYPE体力值恢复速度 = "体力值恢复速度";
    private const string TYPE护盾爆率 = "护盾爆率";
    private const string TYPE血瓶爆率 = "血瓶爆率";
    private const string TYPE金币爆率 = "金币爆率";
    private const string TYPE祝福固定增长 = "祝福固定增长";
    private const string TYPE祝福击杀增长 = "祝福击杀增长";
    private const string TYPE生命窃取率 = "生命窃取率";
    private const string TYPE闪避率 = "闪避率";
    void Start()
    {
        // 1. 初始化属性 (非常重要！确保游戏开始时，玩家身上的属性是加上了强化值的)
        初始化玩家属性数值();

        // 2. 绑定选择按钮事件
        if (生命值选择按钮 != null) 生命值选择按钮.onClick.AddListener(选择生命值);
        if (护甲值选择按钮 != null) 护甲值选择按钮.onClick.AddListener(选择护甲值);
        if (体力值选择按钮 != null) 体力值选择按钮.onClick.AddListener(选择体力值);
        if (护甲恢复延迟选择按钮 != null) 护甲恢复延迟选择按钮.onClick.AddListener(选择护甲恢复延迟);
        if (护甲恢复速度选择按钮 != null) 护甲恢复速度选择按钮.onClick.AddListener(选择护甲恢复速度);
        if (护盾时长选择按钮 != null) 护盾时长选择按钮.onClick.AddListener(选择护盾时长);
        if (体力值恢复速度选择按钮 != null) 体力值恢复速度选择按钮.onClick.AddListener(选择体力值恢复速度);
        if (护盾爆率选择按钮 != null) 护盾爆率选择按钮.onClick.AddListener(选择护盾爆率);
        if (血瓶爆率选择按钮 != null) 血瓶爆率选择按钮.onClick.AddListener(选择血瓶爆率);
        if (金币爆率选择按钮 != null) 金币爆率选择按钮.onClick.AddListener(选择金币爆率);
        if (祝福固定增长选择按钮 != null) 祝福固定增长选择按钮.onClick.AddListener(选择祝福固定增长);
        if (祝福击杀增长选择按钮 != null) 祝福击杀增长选择按钮.onClick.AddListener(选择祝福击杀增长);
        if (闪避率选择按钮 != null) 闪避率选择按钮.onClick.AddListener(选择闪避率);
        if (生命窃取率选择按钮 != null) 生命窃取率选择按钮.onClick.AddListener(选择生命窃取率);

        // 3. 绑定强化按钮事件
        if (强化按钮 != null) 强化按钮.onClick.AddListener(执行强化);

        显示界面.gameObject.SetActive(false);
        提示文字.gameObject.SetActive(true);
    }
    void 初始化玩家属性数值()
    {
        if (玩家属性.Instance == null) return;

        int 生命值等级 = PlayerPrefs.GetInt(生命值等级KEY, 0);
        玩家属性.Instance.最大生命值 += (int)(生命值等级 * 生命值每次增加量);

        int 护甲值等级 = PlayerPrefs.GetInt(护甲值等级KEY, 0);
        玩家属性.Instance.最大护甲值 += (int)(护甲值等级 * 护甲值每次增加量);

        int 体力值等级 = PlayerPrefs.GetInt(体力值等级KEY, 0);
        玩家属性.Instance.最大体力值 += (体力值等级 * 体力值每次增加量);

        int 护甲恢复延迟等级 = PlayerPrefs.GetInt(护甲恢复延迟等级KEY, 0);
        玩家属性.Instance.护甲恢复延迟 += (护甲恢复延迟等级 * 护甲恢复延迟每次增加量);

        int 护甲恢复速度等级 = PlayerPrefs.GetInt(护甲恢复速度等级KEY, 0);
        玩家属性.Instance.护甲恢复速度 += (护甲恢复速度等级 * 护甲恢复速度每次增加量);

        int 护盾时长等级 = PlayerPrefs.GetInt(护盾时长等级KEY, 0);
        玩家属性.Instance.护盾时长 += (护盾时长等级 * 护盾时长每次增加量);

        int 体力值恢复速度等级 = PlayerPrefs.GetInt(体力值恢复速度等级KEY, 0);
        玩家属性.Instance.体力值恢复速度 += (体力值恢复速度等级 * 体力值恢复速度每次增加量);

        int 护盾爆率等级 = PlayerPrefs.GetInt(护盾爆率等级KEY, 0);
        玩家属性.Instance.护盾爆率 += (护盾爆率等级 * 护盾爆率每次增加量);

        int 血瓶爆率等级 = PlayerPrefs.GetInt(血瓶爆率等级KEY, 0);
        玩家属性.Instance.血瓶爆率 += (血瓶爆率等级 * 血瓶爆率每次增加量);

        int 金币爆率等级 = PlayerPrefs.GetInt(金币爆率等级KEY, 0);
        玩家属性.Instance.金币爆率 += (金币爆率等级 * 金币爆率每次增加量);

        int 祝福固定增长等级 = PlayerPrefs.GetInt(祝福固定增长等级KEY, 0);
        玩家属性.Instance.祝福固定增长 += (祝福固定增长等级 * 祝福固定增长每次增加量);

        int 祝福击杀增长等级 = PlayerPrefs.GetInt(祝福击杀增长等级KEY, 0);
        玩家属性.Instance.祝福击杀增长 += (祝福击杀增长等级 * 祝福击杀增长每次增加量);

        int 生命窃取率等级 = PlayerPrefs.GetInt(生命窃取率等级KEY, 0);
        玩家属性.Instance.生命窃取率 += (生命窃取率等级 * 生命窃取率每次增加量);

        int 闪避率等级 = PlayerPrefs.GetInt(闪避率等级KEY, 0);
        玩家属性.Instance.闪避率 += (闪避率等级 * 闪避率每次增加量);
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
        更新UI显示("最大护甲值", "增加角色的最大护甲值上限。", 护甲值Sprite,
            护甲值等级KEY, 玩家属性.Instance.最大护甲值, 护甲值每次增加量);
    }

    void 选择体力值()
    {
        当前选择的属性类型 = TYPE体力值;
        更新UI显示("最大体力值", "增加冲锋所需的最大体力值上限。", 体力值Sprite,
            体力值等级KEY, 玩家属性.Instance.最大体力值, 体力值每次增加量);
    }
    void 选择护甲恢复延迟()
    {
        当前选择的属性类型 = TYPE护甲恢复延迟;
        更新UI显示("护甲恢复延迟", "减少护甲开始恢复所需没有受到伤害的时间。", 护甲恢复延迟Sprite,
            护甲恢复延迟等级KEY, 玩家属性.Instance.护甲恢复延迟, 护甲恢复延迟每次增加量);
    }
    void 选择护甲恢复速度()
    {
        当前选择的属性类型 = TYPE护甲恢复速度;
        更新UI显示("护甲恢复速度", "加快角色的护甲的恢复速度。", 护甲恢复速度Sprite,
            护甲恢复速度等级KEY, 玩家属性.Instance.护甲恢复速度, 护甲恢复速度每次增加量);
    }
    void 选择护盾时长()
    {
        当前选择的属性类型 = TYPE护盾时长;
        更新UI显示("护盾时长", "延长护盾道具提供的护盾时间。", 护盾时长Sprite,
            护盾时长等级KEY, 玩家属性.Instance.护盾时长, 护盾时长每次增加量);
    }
    void 选择体力值恢复速度()
    {
        当前选择的属性类型 = TYPE体力值恢复速度;
        更新UI显示("体力值恢复速度", "加快角色的体力值恢复速度。", 体力值恢复速度Sprite,
            体力值恢复速度等级KEY, 玩家属性.Instance.体力值恢复速度, 体力值恢复速度每次增加量);
    }
    void 选择护盾爆率()
    {
        当前选择的属性类型 = TYPE护盾爆率;
        更新UI显示("护盾爆率", "增加击杀敌人后掉落护盾的概率。", 护盾爆率Sprite,
            护盾爆率等级KEY, 玩家属性.Instance.护盾爆率, 护盾爆率每次增加量);
    }
    void 选择血瓶爆率()
    {
        当前选择的属性类型 = TYPE血瓶爆率;
        更新UI显示("血瓶爆率", "增加击杀敌人后掉落血瓶的概率。", 血瓶爆率Sprite,
            血瓶爆率等级KEY, 玩家属性.Instance.血瓶爆率, 血瓶爆率每次增加量);
    }
    void 选择金币爆率()
    {
        当前选择的属性类型 = TYPE金币爆率;
        更新UI显示("金币爆率", "增加击杀敌人后掉落金币的概率。", 金币爆率Sprite,
            金币爆率等级KEY, 玩家属性.Instance.金币爆率, 金币爆率每次增加量);
    }
    void 选择祝福固定增长()
    {
        当前选择的属性类型 = TYPE祝福固定增长;
        更新UI显示("祝福固定增长", "加快祝福随时间增长的速度。", 祝福固定增长Sprite,
            祝福固定增长等级KEY, 玩家属性.Instance.祝福固定增长, 祝福固定增长每次增加量);
    }
    void 选择祝福击杀增长()
    {
        当前选择的属性类型 = TYPE祝福击杀增长;
        更新UI显示("祝福击杀增长", "增加击杀敌人后获得的祝福。", 祝福击杀增长Sprite,
            祝福击杀增长等级KEY, 玩家属性.Instance.祝福击杀增长, 祝福击杀增长每次增加量);
    }

    void 选择生命窃取率()
    {
        当前选择的属性类型 = TYPE生命窃取率;
        更新UI显示("生命窃取率", "增加攻击敌人后恢复自身生命值的概率", 生命窃取率Sprite,
            生命窃取率等级KEY, 玩家属性.Instance.生命窃取率, 生命窃取率每次增加量);
    }
    void 选择闪避率()
    {
        当前选择的属性类型 = TYPE闪避率;
        更新UI显示("闪避率", "增加角色受到敌人攻击但不受到伤害的概率。", 闪避率Sprite,
            闪避率等级KEY, 玩家属性.Instance.闪避率, 闪避率每次增加量);
    }
    void 更新UI显示(string name, string desc, Sprite sprite, string saveKey, float currentVal, float increaseAmount)
    {
        显示界面.gameObject.SetActive(true);
        提示文字.gameObject.SetActive(false);
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
            case TYPE护甲恢复延迟:
                key = 护甲恢复延迟等级KEY;
                increaseAmt = 护甲恢复延迟每次增加量;
                refreshAction = 选择护甲恢复延迟;
                break;
            case TYPE护甲恢复速度:
                key = 护甲恢复速度等级KEY;
                increaseAmt = 护甲恢复速度每次增加量;
                refreshAction = 选择护甲恢复速度;
                break;
            case TYPE护盾时长:
                key = 护盾时长等级KEY;
                increaseAmt = 护盾时长每次增加量;
                refreshAction = 选择护盾时长;
                break;
            case TYPE体力值恢复速度:
                key = 体力值恢复速度等级KEY;
                increaseAmt = 体力值恢复速度每次增加量;
                refreshAction = 选择体力值恢复速度;
                break;
            case TYPE护盾爆率:
                key = 护盾爆率等级KEY;
                increaseAmt = 护盾爆率每次增加量;
                refreshAction = 选择护盾爆率;
                break;
            case TYPE血瓶爆率:
                key = 血瓶爆率等级KEY;
                increaseAmt = 血瓶爆率每次增加量;
                refreshAction = 选择血瓶爆率;
                break;
            case TYPE金币爆率:
                key = 金币爆率等级KEY;
                increaseAmt = 金币爆率每次增加量;
                refreshAction = 选择金币爆率;
                break;
            case TYPE祝福固定增长:
                key = 祝福固定增长等级KEY;
                increaseAmt = 祝福固定增长每次增加量;
                refreshAction = 选择祝福固定增长;
                break;
            case TYPE祝福击杀增长:
                key = 祝福击杀增长等级KEY;
                increaseAmt = 祝福击杀增长每次增加量;
                refreshAction = 选择祝福击杀增长;
                break;
            case TYPE生命窃取率:
                key = 生命窃取率等级KEY;
                increaseAmt = 生命窃取率每次增加量;
                refreshAction = 选择生命窃取率;
                break;
            case TYPE闪避率:
                key = 闪避率等级KEY;
                increaseAmt = 闪避率每次增加量;
                refreshAction = 选择闪避率;
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
                case TYPE生命值: 玩家属性.Instance.最大生命值 += increaseAmt; break;
                case TYPE护甲值: 玩家属性.Instance.最大护甲值 += increaseAmt; break;
                case TYPE体力值: 玩家属性.Instance.最大体力值 += increaseAmt; break;
                case TYPE护甲恢复延迟: 玩家属性.Instance.护甲恢复延迟 += increaseAmt; break;
                case TYPE护甲恢复速度: 玩家属性.Instance.护甲恢复速度 += increaseAmt; break;
                case TYPE护盾时长: 玩家属性.Instance.护盾时长 += increaseAmt; break;
                case TYPE体力值恢复速度: 玩家属性.Instance.体力值恢复速度 += increaseAmt; break;
                case TYPE护盾爆率: 玩家属性.Instance.护盾爆率 += increaseAmt; break;
                case TYPE血瓶爆率: 玩家属性.Instance.血瓶爆率 += increaseAmt; break;
                case TYPE金币爆率: 玩家属性.Instance.金币爆率 += increaseAmt; break;
                case TYPE祝福固定增长: 玩家属性.Instance.祝福固定增长 += increaseAmt; break;
                case TYPE祝福击杀增长: 玩家属性.Instance.祝福击杀增长 += increaseAmt; break;
                case TYPE生命窃取率: 玩家属性.Instance.生命窃取率 += increaseAmt; break;
                case TYPE闪避率: 玩家属性.Instance.闪避率 += increaseAmt; break;
            }

            // 5. 保存等级
            PlayerPrefs.SetInt(key, currentLevel + 1);
            PlayerPrefs.Save();
            音频管理器.Instance.播放强化音效();
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
