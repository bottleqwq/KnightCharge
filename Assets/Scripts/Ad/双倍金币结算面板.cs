using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

namespace KnightCharge.Ad
{
    /// <summary>
    /// 战败退出时的「看视频双倍金币」结算界面控制器
    /// 挂载在游戏战斗场景的 双倍金币面板 上
    /// </summary>
    public class 双倍金币结算面板 : MonoBehaviour
    {
        [Header("面板引用")]
        [Tooltip("双倍金币面板自身根物体（若为空则默认取自身 GameObject）")]
        public GameObject 双倍金币面板;

        [Tooltip("阵亡面板（显示结算时自动隐藏阵亡面板）")]
        public GameObject 阵亡面板;

        [Header("UI 文本")]
        [Tooltip("显示本局获得金币的文本")]
        public TextMeshProUGUI 本局金币文本;

        [Tooltip("显示看视频翻倍提示的文本")]
        public TextMeshProUGUI 翻倍提示文本;

        [Header("UI 按钮")]
        [Tooltip("观看视频获取双倍金币按钮")]
        public Button 看视频双倍按钮;

        [Tooltip("放弃双倍直接退出按钮")]
        public Button 直接退出按钮;

        [Header("配置")]
        [Tooltip("当本局未收集到金币时的保底翻倍金币奖励")]
        public int 保底翻倍金币 = 5;

        [Tooltip("退出后跳转的目标场景名称")]
        public string 返回场景名称 = "备战界面";

        private void Awake()
        {
            if (双倍金币面板 == null)
            {
                双倍金币面板 = gameObject;
            }
        }

        private void Start()
        {
            if (看视频双倍按钮 != null)
            {
                看视频双倍按钮.onClick.AddListener(点击看视频双倍);
            }

            if (直接退出按钮 != null)
            {
                直接退出按钮.onClick.AddListener(点击直接退出);
            }
        }

        /// <summary>
        /// 供阵亡面板的「退出战斗」按钮调用，弹出双倍金币结算界面
        /// </summary>
        public void 显示面板()
        {
            Debug.Log("[双倍金币结算] 打开双倍金币结算面板");

            // 播放按钮音效
            if (音频管理器.Instance != null)
            {
                音频管理器.Instance.播放按钮点击音效();
            }

            // 隐藏阵亡面板
            if (阵亡面板 != null)
            {
                阵亡面板.SetActive(false);
            }

            // 打开双倍金币面板
            if (双倍金币面板 != null)
            {
                双倍金币面板.SetActive(true);
            }
            else
            {
                gameObject.SetActive(true);
            }

            // 读取本局金币并刷新显示
            int earnedCoins = (玩家属性.Instance != null) ? 玩家属性.Instance.本局获得金币 : 0;
            int bonusCoins = (earnedCoins > 0) ? earnedCoins : 保底翻倍金币;

            if (本局金币文本 != null)
            {
                本局金币文本.text = $"本局获得金币: <color=#FFD700>{earnedCoins}</color>";
            }

            if (翻倍提示文本 != null)
            {
                翻倍提示文本.text = $"观看视频广告\n可额外获得 <color=#FFD700>+{bonusCoins}</color> 金币！";
            }

            SetButtonsInteractable(true);
        }

        /// <summary>
        /// 点击「看视频双倍领取」按钮
        /// </summary>
        public void 点击看视频双倍()
        {
            Debug.Log("[双倍金币结算] 用户点击看视频双倍领取");

            if (音频管理器.Instance != null)
            {
                音频管理器.Instance.播放按钮点击音效();
            }

            // 禁用按钮防连击
            SetButtonsInteractable(false);

            // 超时保护
            CancelInvoke(nameof(超时恢复按钮));
            Invoke(nameof(超时恢复按钮), 10f);

            // 确保视频广告管理器单例存在
            if (DirichletRewardVideoManager.Instance == null)
            {
                var mgrGo = new GameObject("DirichletRewardVideoManager");
                mgrGo.AddComponent<DirichletRewardVideoManager>();
            }

            // 调用激励视频展示
            DirichletRewardVideoManager.Instance.ShowRewardVideo(
                onRewardSuccess: () =>
                {
                    int earnedCoins = (玩家属性.Instance != null) ? 玩家属性.Instance.本局获得金币 : 0;
                    int bonusCoins = (earnedCoins > 0) ? earnedCoins : 保底翻倍金币;

                    // 发放双倍奖励（计入本局 = false，防止重复累加本局金币统计）
                    if (玩家属性.Instance != null)
                    {
                        玩家属性.Instance.增加金币(bonusCoins, false);
                        Debug.Log($"<color=green>[双倍金币结算] 视频观看成功！发放翻倍金币: +{bonusCoins}，当前总金币: {玩家属性.Instance.当前金币}</color>");
                    }

                    if (音频管理器.Instance != null)
                    {
                        音频管理器.Instance.播放强化音效();
                    }

                    // 恢复时间并返回备战界面
                    Time.timeScale = 1f;
                    SceneManager.LoadScene(返回场景名称);
                },
                onAdClosed: () =>
                {
                    Debug.Log("[双倍金币结算] 广告已关闭");
                    CancelInvoke(nameof(超时恢复按钮));
                    SetButtonsInteractable(true);
                },
                onAdFailed: (errorCode, errorMessage) =>
                {
                    Debug.LogWarning($"[双倍金币结算] 广告加载或展示失败: [{errorCode}] {errorMessage}");
                    CancelInvoke(nameof(超时恢复按钮));
                    SetButtonsInteractable(true);
                }
            );
        }

        /// <summary>
        /// 点击「直接退出」按钮，放弃双倍直接返回备战界面
        /// </summary>
        public void 点击直接退出()
        {
            Debug.Log("[双倍金币结算] 用户选择直接退出");

            if (音频管理器.Instance != null)
            {
                音频管理器.Instance.播放按钮点击音效();
            }

            Time.timeScale = 1f;
            SceneManager.LoadScene(返回场景名称);
        }

        private void 超时恢复按钮()
        {
            Debug.LogWarning("[双倍金币结算] 广告回调等待超时，恢复按钮交互");
            SetButtonsInteractable(true);
        }

        private void SetButtonsInteractable(bool state)
        {
            if (看视频双倍按钮 != null)
            {
                看视频双倍按钮.interactable = state;
            }

            if (直接退出按钮 != null)
            {
                直接退出按钮.interactable = state;
            }
        }

        private void OnDestroy()
        {
            if (看视频双倍按钮 != null)
            {
                看视频双倍按钮.onClick.RemoveListener(点击看视频双倍);
            }

            if (直接退出按钮 != null)
            {
                直接退出按钮.onClick.RemoveListener(点击直接退出);
            }
        }
    }
}
