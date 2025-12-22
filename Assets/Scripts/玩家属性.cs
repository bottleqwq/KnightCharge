using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class 玩家属性 : MonoBehaviour
{
    public static 玩家属性 Instance { get; private set; }

    [Header("属性")]
    public float 最大体力值 = 100f;
    public float 体力恢复 = 10f;
    public int 最大生命值 = 10;
    public float 护甲恢复速度 = 1f;
    public float 护甲恢复延迟 = 3f;
    public int 最大护甲值 = 5;
    public float 拾取范围 = 0;
    public float 祝福需要值 = 100f;
    public float 祝福固定增长值 = 5f;
    public float 祝福击杀增长值 = 20f;
    public int 当前金币 = 0;

    [Header("UI 设置")]
    public TextMeshProUGUI 金币显示文本;

    private const string 保存金币KEY = "骑士冲锋金币数量";
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    private void Start()
    {
        当前金币 = PlayerPrefs.GetInt(保存金币KEY, 0);
        更新金币显示文本();
    }
    void 更新金币显示文本()
    {
        if (金币显示文本 != null)
        {
            金币显示文本.text = "金币: " + 当前金币.ToString();
        }
    }
    public void 增加金币(int amount)
    {
        当前金币 += amount;
        更新金币显示文本();
        PlayerPrefs.SetInt(保存金币KEY, 当前金币);
        PlayerPrefs.Save();
        Debug.Log("当前金币: " + 当前金币);
    }

    public void 注册金币显示文本(TextMeshProUGUI newTextComponent)
    {
        金币显示文本 = newTextComponent;
        更新金币显示文本(); // 刚连接上，立刻刷新一次显示当前金额
    }

    public bool 扣除金币(int amount)
    {
        if (当前金币 >= amount)
        {
            当前金币 -= amount;

            // 保存金币
            PlayerPrefs.SetInt(保存金币KEY, 当前金币);
            PlayerPrefs.Save();

            更新金币显示文本();
            return true;
        }
        return false;
    }
    // 辅助画线，方便调试
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, 拾取范围);
    }
}
