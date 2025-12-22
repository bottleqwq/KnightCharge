using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

public class 祝福管理器 : MonoBehaviour
{
    public static 祝福管理器 Instance;

    [Header("UI 引用")]
    public Slider 祝福条;
    public GameObject 强化面板;
    public Button[] 强化;
    // 如果按钮里有文本组件，也可以在这里引用，用来修改文字

    private float 当前祝福值 = 0f;
    private bool isPaused = false;
    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
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
        增加祝福值(玩家属性.Instance.祝福固定增长值 * Time.deltaTime);
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
        // 这里简单模拟3个选项，实际开发中你应该有一个 UpgradeData 的列表
        string[] upgrades = { "攻击力 +10%", "移动速度 +10%", "攻击范围 +15%", "冷却缩减 -5%" };

        for (int i = 0; i < 强化.Length; i++)
        {
            int index = i; // 闭包捕获索引

            // 随机获取一个技能名 (简单演示)
            string skillName = upgrades[Random.Range(0, upgrades.Length)];

            // 修改按钮上的文字 (假设按钮下第一个子物体是 Text)
            // 如果用的是 TMP_Text，请更改类型
            var textComp = 强化[i].GetComponentInChildren<TextMeshProUGUI>();
            if (textComp != null) textComp.text = skillName;
            else 强化[i].GetComponentInChildren<Text>().text = skillName;

            // 清除之前的点击事件，防止堆积
            强化[i].onClick.RemoveAllListeners();

            // 绑定点击事件：选中后执行逻辑
            强化[i].onClick.AddListener(() => 选择强化(skillName));
        }
    }

    // 玩家点击某个选项后
    void 选择强化(string upgradeName)
    {
        Debug.Log("玩家选择了: " + upgradeName);

        // TODO: 在这里写具体增加属性的逻辑
        // e.g. if(upgradeName == "攻击力 +10%") Player.Attack += 10;

        // 恢复游戏流程
        当前祝福值 = 0f; // 重置祝福值
        // 如果想要类似吸血鬼幸存者那样每次升级所需经验变多，可以在这里增加 maxBlessing
        // maxBlessing *= 1.2f; 

        更新祝福条();
        强化面板.SetActive(false);
        Time.timeScale = 1f; // 恢复时间
        isPaused = false;
    }
}
