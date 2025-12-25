using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class 祝福管理器 : MonoBehaviour
{
    public static 祝福管理器 Instance;

    [Header("UI 引用")]
    public Slider 祝福条;
    public GameObject 强化面板;
    public Button[] 强化;
    // 如果按钮里有文本组件，也可以在这里引用，用来修改文字

    [Header("数据源")]
    public List<UpgradeData> 所有强化;

    private Dictionary<UpgradeData, int> 强化次数 = new Dictionary<UpgradeData, int>();

    private float 当前祝福值 = 0f;
    private bool isPaused = false;
    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
        强化次数.Clear();
    }

    void Start()
    {
        更新祝福条();
        强化面板.SetActive(false); // 确保开始时面板是隐藏的
    }
    void Update()
    {
        // 如果游戏暂停（正在选技能），就不增加数值
        if (isPaused) return;

        // 1. 每秒增加 5 点 (使用 Time.deltaTime 实现平滑增长)
        增加祝福值(玩家属性.Instance.祝福固定增长 * Time.deltaTime);
    }

    // 增加祝福值的通用方法
    public void 增加祝福值(float amount)
    {
        if (isPaused) return;

        当前祝福值 += amount;

        // 检查是否达到阈值
        if (当前祝福值 >= 玩家属性.Instance.祝福需要值)
        {
            当前祝福值 = 玩家属性.Instance.祝福需要值; // 防止溢出显示
            触发女神的祝福();      // 触发升级
        }

        更新祝福条();
    }

    void 更新祝福条()
    {
        if (祝福条 != null)
        {
            祝福条.value = 当前祝福值 / 玩家属性.Instance.祝福需要值;
        }
    }

    void 触发女神的祝福()
    {
        isPaused = true;
        Time.timeScale = 0f; // 暂停游戏时间

        生成强化选项(); // 生成随机选项
        强化面板.SetActive(true); // 显示面板
    }

    // 生成3个随机选项并绑定点击事件
    void 生成强化选项()
    {
        // 筛选出一个“候选池”
        // 规则：如果没有达到最大次数限制，才可以放入池子
        List<UpgradeData> 候选池 = new List<UpgradeData>();

        foreach (var data in 所有强化)
        {
            // 获取当前这个技能选了几次，如果没有记录过，就是 0
            int currentCount = 强化次数.ContainsKey(data) ? 强化次数[data] : 0;

            // 如果 maxCount 是 -1 (无限)，或者当前次数 < 最大次数
            if (data.maxCount == -1 || currentCount < data.maxCount)
            {
                候选池.Add(data);
            }
        }

        // 抽取逻辑 (和之前类似，只是源变成了 validPool)
        int optionCount = Mathf.Min(3, 候选池.Count);

        for (int i = 0; i < 强化.Length; i++)
        {
            if (i >= optionCount)
            {
                强化[i].gameObject.SetActive(false);
                continue;
            }
            强化[i].gameObject.SetActive(true);

            // 随机抽取
            int randomIndex = Random.Range(0, 候选池.Count);
            UpgradeData selectedData = 候选池[randomIndex];
            候选池.RemoveAt(randomIndex); // 抽走防止重复

            // 4. 【新】更新 UI (包含稀有度视觉效果)
            更新视觉效果(强化[i], selectedData);

            // 绑定事件
            强化[i].onClick.RemoveAllListeners();
            强化[i].onClick.AddListener(() => 选择强化(selectedData));
        }
    }

    void 更新视觉效果(Button btn, UpgradeData data)
    {
        // 获取组件引用
        TextMeshProUGUI titleText = btn.transform.Find("TitleText").GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI descText = btn.transform.Find("DescText").GetComponent<TextMeshProUGUI>();
        Image iconImg = btn.transform.Find("Icon").GetComponent<Image>();
        Image bgImage = btn.GetComponent<Image>(); // 假设按钮背景图就是卡片底板

        // 设置基本信息
        titleText.text = data.强化名称;
        descText.text = GetDescriptionWithLevel(data); // 进阶：可以在描述里显示 (1/3) 这种进度
        iconImg.sprite = data.强化图标;

        // 【稀有度视觉处理】
        // 这里简单用颜色区分，你可以换成替换 Sprite 边框图片
        switch (data.rarity)
        {
            case Rarity.普通:
                bgImage.color = new Color(0.8f, 0.8f, 0.8f); // 灰色
                titleText.color = Color.white;
                break;
            case Rarity.稀有:
                bgImage.color = new Color(0.4f, 0.6f, 1f); // 蓝色
                titleText.color = Color.cyan;
                break;
            case Rarity.史诗:
                bgImage.color = new Color(1f, 0.8f, 0.2f); // 金色
                titleText.color = Color.yellow;
                break;
        }
    }

    // 辅助：生成带等级的描述，例如 "闪避率 +5% (1/3)"
    string GetDescriptionWithLevel(UpgradeData data)
    {
        if (data.maxCount == -1) return data.强化介绍;

        int current = 强化次数.ContainsKey(data) ? 强化次数[data] : 0;
        return $"{data.强化介绍}\n<size=80%>({current}/{data.maxCount})</size>";
    }

    // 玩家点击某个选项后
    void 选择强化(UpgradeData data)
    {
        if (强化次数.ContainsKey(data))
            强化次数[data]++;
        else
            强化次数[data] = 1;

        Debug.Log($"玩家选择了: {data.强化名称}");

        应用强化(data);

        // 恢复游戏流程
        当前祝福值 = 0f; // 重置祝福值
        // 如果想要类似吸血鬼幸存者那样每次升级所需经验变多，可以在这里增加 maxBlessing
        // maxBlessing *= 1.2f; 

        更新祝福条();
        强化面板.SetActive(false);
        Time.timeScale = 1f; // 恢复时间
        isPaused = false;
    }
    void 应用强化(UpgradeData data)
    {
        switch (data.type)
        {
            case UpgradeType.生命值强化:
                玩家属性.Instance.最大生命值 += data.数值;
                break;
            case UpgradeType.体力值强化:
                玩家属性.Instance.最大体力值 += data.数值;
                break;
            case UpgradeType.护甲值强化:
                玩家属性.Instance.最大护甲值 += data.数值;
                break;
            case UpgradeType.拾取范围强化:
                玩家属性.Instance.拾取范围 += data.数值;
                break;
            case UpgradeType.金币爆率强化:
                玩家属性.Instance.金币爆率 += data.数值;
                break;
        }
    }
}
