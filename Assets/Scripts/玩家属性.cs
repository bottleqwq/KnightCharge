using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class 玩家属性 : MonoBehaviour
{
    public static 玩家属性 Instance { get; private set; }

    [Header("配置基础数值")]
    public float _基础最大体力值 = 100f;
    public float _基础体力值恢复速度 = 5f;
    public float _基础最大生命值 = 10f;
    public float _基础护甲恢复速度 = 0.3f;
    public float _基础护甲恢复延迟 = 5f;
    public float _基础最大护甲值 = 5;
    public float _基础拾取范围 = 1;
    public float _基础祝福需要值 = 1000f;
    public float _基础祝福固定增长 = 5f;
    public float _基础祝福击杀增长 = 20f;
    public float _基础金币爆率 = 50f;
    public float _基础血瓶爆率 = 50f;
    public float _基础血瓶恢复值 = 1f;
    public float _基础护盾爆率 = 50f;
    public float _基础护盾时长 = 3f;
    public float _基础闪避率 = 5f;
    public float _基础生命窃取率 = 5f;
    public float _基础生命窃取值 = 1f;

    [Header("实时运行时数值")]
    public float 最大体力值;
    public float 体力值恢复速度;
    public float 最大生命值;
    public float 护甲恢复速度;
    public float 护甲恢复延迟;
    public float 最大护甲值;
    public float 拾取范围;
    public float 祝福需要值;
    public float 祝福固定增长;
    public float 祝福击杀增长;
    public float 金币爆率;
    public float 血瓶爆率;
    public float 血瓶恢复值;
    public float 护盾爆率;
    public float 护盾时长;
    public float 闪避率;
    public float 生命窃取率;
    public float 生命窃取值;

    [Header("累加型数值")]
    public float 基础伤害增加值;
    public float 蓄力伤害增加值;
    public float 旋转速度修正值;
    public float 击退增加值;
    public float 冲刺力度增加值;

    [Header("当前金币数量")]
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
        重置属性();
    }
    private void Start()
    {
        当前金币 = PlayerPrefs.GetInt(保存金币KEY, 0);
        更新金币显示文本();
    }
    public void 重置属性()
    {
        最大体力值 = _基础最大体力值;
        体力值恢复速度 = _基础体力值恢复速度;
        最大生命值 = _基础最大生命值;
        护甲恢复速度 = _基础护甲恢复速度;
        护甲恢复延迟 = _基础护甲恢复延迟;
        最大护甲值 = _基础最大护甲值;
        拾取范围 = _基础拾取范围;
        祝福需要值 = _基础祝福需要值;
        祝福固定增长 = _基础祝福固定增长;
        祝福击杀增长 = _基础祝福击杀增长;
        金币爆率 = _基础金币爆率;
        血瓶爆率 = _基础血瓶爆率;
        血瓶恢复值 = _基础血瓶恢复值;
        护盾爆率 = _基础护盾爆率;
        护盾时长 = _基础护盾时长;
        闪避率 = _基础闪避率;
        生命窃取率 = _基础生命窃取率;
        生命窃取值 = _基础生命窃取率;

        基础伤害增加值 = 0f;
        蓄力伤害增加值 = 0f;
        旋转速度修正值 = 0f;
        击退增加值 = 0f;
        冲刺力度增加值 = 0f;
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
