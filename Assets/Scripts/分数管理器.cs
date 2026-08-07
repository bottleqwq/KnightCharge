using TMPro;
using UnityEngine;

public class 分数管理器 : MonoBehaviour
{
    public static 分数管理器 Instance { get; private set; }

    public int 当前得分;
    public int 历史最高得分 { get; private set; }

    public TextMeshProUGUI 当前得分文本;
    public TextMeshProUGUI 最高得分文本;

    private const string 保存最高分KEY = "骑士冲锋历史最高得分";

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        当前得分 = 0;
        历史最高得分 = PlayerPrefs.GetInt(保存最高分KEY, 0);
    }

    private void Start()
    {
        更新显示();
    }

    public void 增加分数(int amount)
    {
        当前得分 += amount;

        if (当前得分 > 历史最高得分)
        {
            历史最高得分 = 当前得分;
            PlayerPrefs.SetInt(保存最高分KEY, 历史最高得分);
            PlayerPrefs.Save();
        }

        更新显示();
    }

    public void 注册当前得分文本(TextMeshProUGUI text)
    {
        当前得分文本 = text;
        更新显示();
    }

    public void 注册最高得分文本(TextMeshProUGUI text)
    {
        最高得分文本 = text;
        更新显示();
    }

    private void 更新显示()
    {
        if (当前得分文本 != null)
        {
            当前得分文本.text = $"当前得分: {当前得分}";
        }

        if (最高得分文本 != null)
        {
            最高得分文本.text = $"最高得分: {历史最高得分}";
        }
    }
}
