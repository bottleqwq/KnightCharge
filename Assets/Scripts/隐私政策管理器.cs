using UnityEngine;
using UnityEngine.UI;
#if TMP_PRESENT || UNITY_TEXTMESHPRO
using TMPro;
#endif

/// <summary>
/// 《骑士冲锋》隐私政策弹窗控制器
/// 负责在游戏首次启动时弹出合规的隐私政策授权窗口，管理用户同意/拒绝状态及隐私链接跳转。
/// </summary>
public class 隐私政策管理器 : MonoBehaviour
{
    public static 隐私政策管理器 Instance { get; private set; }

    [Header("【UI 面板引用】")]
    [Tooltip("隐私政策主弹窗面板（首次启动必须显示）")]
    [SerializeField] private GameObject 隐私政策弹窗面板;

    [Tooltip("二次确认弹窗面板（点击不同意时弹出，可选）")]
    [SerializeField] private GameObject 二次确认退出面板;

    [Header("【按钮与链接配置】")]
    [Tooltip("同意按钮")]
    [SerializeField] private Button 同意按钮;

    [Tooltip("不同意按钮")]
    [SerializeField] private Button 不同意按钮;

    [Tooltip("查看隐私政策全文的按钮（点击后跳转浏览器或内嵌打开）")]
    [SerializeField] private Button 查看隐私政策链接按钮;

    [Tooltip("二次确认面板中的【退出游戏】按钮")]
    [SerializeField] private Button 确认退出按钮;

    [Tooltip("二次确认面板中的【再想想/返回】按钮")]
    [SerializeField] private Button 重新考虑按钮;

    [Header("【隐私政策在线链接】")]
    [Tooltip("您的隐私政策在线托管地址（请填入生成的公开可访问链接）")]
    [SerializeField] private string 隐私政策在线URL = "https://your-privacy-policy-link.html";

    // PlayerPrefs 存储 Key（若未来隐私政策有重大版本更新，可修改此 key 如 PrivacyAgreed_v2 触发重新授权）
    private const string PRIVACY_KEY = "PrivacyPolicyAgreed_v1";

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
            return;
        }
    }

    private void Start()
    {
        // 绑定按钮事件
        if (同意按钮 != null) 同意按钮.onClick.AddListener(OnAgreeClicked);
        if (不同意按钮 != null) 不同意按钮.onClick.AddListener(OnRejectClicked);
        if (查看隐私政策链接按钮 != null) 查看隐私政策链接按钮.onClick.AddListener(OpenPrivacyPolicyURL);
        if (确认退出按钮 != null) 确认退出按钮.onClick.AddListener(ExitGame);
        if (重新考虑按钮 != null) 重新考虑按钮.onClick.AddListener(OnReconsiderClicked);

        // 检查用户是否已同意隐私政策
        CheckPrivacyStatus();
    }

    /// <summary>
    /// 检查隐私政策同意状态
    /// </summary>
    public void CheckPrivacyStatus()
    {
        bool hasAgreed = PlayerPrefs.GetInt(PRIVACY_KEY, 0) == 1;

        if (!hasAgreed)
        {
            // 首次启动，未同意：强制显示弹窗
            if (隐私政策弹窗面板 != null)
            {
                隐私政策弹窗面板.SetActive(true);
            }
            if (二次确认退出面板 != null)
            {
                二次确认退出面板.SetActive(false);
            }
        }
        else
        {
            // 已同意：隐藏弹窗，正常运行游戏
            if (隐私政策弹窗面板 != null)
            {
                隐私政策弹窗面板.SetActive(false);
            }
            if (二次确认退出面板 != null)
            {
                二次确认退出面板.SetActive(false);
            }
        }
    }

    /// <summary>
    /// 点击【同意】按钮
    /// </summary>
    public void OnAgreeClicked()
    {
        // 记录同意状态
        PlayerPrefs.SetInt(PRIVACY_KEY, 1);
        PlayerPrefs.Save();

        // 隐藏弹窗
        if (隐私政策弹窗面板 != null)
        {
            隐私政策弹窗面板.SetActive(false);
        }
        if (二次确认退出面板 != null)
        {
            二次确认退出面板.SetActive(false);
        }

        // 播放按钮点击音效（如果有音频管理器）
        if (音频管理器.Instance != null)
        {
            音频管理器.Instance.播放按钮点击音效();
        }

        Debug.Log("用户已同意隐私政策，游戏正常启动。");
    }

    /// <summary>
    /// 点击【不同意/拒绝】按钮
    /// </summary>
    public void OnRejectClicked()
    {
        if (音频管理器.Instance != null)
        {
            音频管理器.Instance.播放按钮点击音效();
        }

        // 如果配置了二次确认退出面板，则展示二次确认；否则直接退出
        if (二次确认退出面板 != null)
        {
            二次确认退出面板.SetActive(true);
        }
        else
        {
            ExitGame();
        }
    }

    /// <summary>
    /// 二次确认面板点击【再想想/返回】
    /// </summary>
    public void OnReconsiderClicked()
    {
        if (音频管理器.Instance != null)
        {
            音频管理器.Instance.播放按钮点击音效();
        }

        if (二次确认退出面板 != null)
        {
            二次确认退出面板.SetActive(false);
        }
    }

    /// <summary>
    /// 打开在线隐私政策网页
    /// </summary>
    public void OpenPrivacyPolicyURL()
    {
        if (!string.IsNullOrEmpty(隐私政策在线URL))
        {
            if (音频管理器.Instance != null)
            {
                音频管理器.Instance.播放按钮点击音效();
            }

            Application.OpenURL(隐私政策在线URL);
        }
        else
        {
            Debug.LogWarning("未配置隐私政策在线 URL！");
        }
    }

    /// <summary>
    /// 主界面“关于/设置”等位置主动调用，方便玩家随时再次查看隐私政策
    /// </summary>
    public void ShowPrivacyPolicyManually()
    {
        if (隐私政策弹窗面板 != null)
        {
            隐私政策弹窗面板.SetActive(true);
        }
    }

    /// <summary>
    /// 退出游戏
    /// </summary>
    public void ExitGame()
    {
        Debug.Log("用户拒绝隐私政策，退出游戏。");
        Application.Quit();
    }
}
