using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace KnightCharge.Ad
{
    /// <summary>
    /// 备战界面「观看激励视频获取金币」业务逻辑组件
    /// 挂载在备战界面的 UI 按钮或 Canvas 节点上
    /// </summary>
    public class 备战界面广告奖励 : MonoBehaviour
    {
        [Header("UI 绑定")]
        [Tooltip("观看广告按钮（若为空则自动获取自身 Button 组件）")]
        public Button 观看广告按钮;

        [Tooltip("按钮上的说明文字（可选，例如显示 '看视频 +10 金币'）")]
        public TextMeshProUGUI 按钮文本;

        [Tooltip("获得金币时的浮动提示/文本 UI 对象")]
        public GameObject 获得奖励提示UI;

        [Header("奖励配置")]
        [Tooltip("每次观看视频获得的金币数量")]
        public int 奖励金币数 = 10;

        [Tooltip("是否在发奖后播放音效")]
        public bool 播放奖励音效 = true;

        private 奖励提示动效 _提示动效组件;

        private void Awake()
        {
            if (观看广告按钮 == null)
            {
                观看广告按钮 = GetComponent<Button>();
            }

            if (获得奖励提示UI != null)
            {
                _提示动效组件 = 获得奖励提示UI.GetComponent<奖励提示动效>();
                if (_提示动效组件 == null)
                {
                    _提示动效组件 = 获得奖励提示UI.AddComponent<奖励提示动效>();
                }
            }
        }

        private void Start()
        {
            if (观看广告按钮 != null)
            {
                观看广告按钮.onClick.AddListener(点击观看视频领金币);
            }

            if (按钮文本 != null)
            {
                按钮文本.text = $"看视频 +{奖励金币数}金币";
            }

            if (获得奖励提示UI != null)
            {
                获得奖励提示UI.SetActive(false);
            }
        }

        /// <summary>
        /// 点击按钮响应方法
        /// </summary>
        public void 点击观看视频领金币()
        {
            Debug.Log($"[AdReward] 用户点击观看激励视频，目标奖励: {奖励金币数} 金币");

            // 播放点击音效
            if (音频管理器.Instance != null)
            {
                音频管理器.Instance.播放按钮点击音效();
            }

            // 防止用户短时间内狂点，暂时禁用按钮
            SetButtonInteractable(false);

            // 设置超时保护，10秒内若未收到任何回调自动恢复按钮
            CancelInvoke(nameof(超时恢复按钮));
            Invoke(nameof(超时恢复按钮), 10f);

            // 确保管理器存在
            if (DirichletRewardVideoManager.Instance == null)
            {
                var mgrGo = new GameObject("DirichletRewardVideoManager");
                mgrGo.AddComponent<DirichletRewardVideoManager>();
            }

            // 调用激励视频展示
            DirichletRewardVideoManager.Instance.ShowRewardVideo(
                onRewardSuccess: () =>
                {
                    // 1. 核心发奖逻辑：给玩家属性增加金币
                    发放金币奖励(奖励金币数);
                },
                onAdClosed: () =>
                {
                    Debug.Log("[AdReward] 广告已关闭，恢复按钮交互");
                    CancelInvoke(nameof(超时恢复按钮));
                    SetButtonInteractable(true);
                },
                onAdFailed: (errorCode, errorMessage) =>
                {
                    Debug.LogWarning($"[AdReward] 广告加载或展示失败: [{errorCode}] {errorMessage}");
                    CancelInvoke(nameof(超时恢复按钮));
                    SetButtonInteractable(true);
                }
            );
        }

        private void 超时恢复按钮()
        {
            Debug.LogWarning("[AdReward] 广告回调等待超时，自动恢复按钮交互");
            SetButtonInteractable(true);
        }

        private void SetButtonInteractable(bool state)
        {
            if (观看广告按钮 != null)
            {
                观看广告按钮.interactable = state;
            }
        }

        /// <summary>
        /// 发放金币奖励并触发飘字动效
        /// </summary>
        private void 发放金币奖励(int amount)
        {
            if (玩家属性.Instance != null)
            {
                玩家属性.Instance.增加金币(amount);
                Debug.Log($"<color=green>[AdReward] 成功发放奖励！金币 +{amount}，当前总金币: {玩家属性.Instance.当前金币}</color>");

                // 播放奖励音效
                if (播放奖励音效 && 音频管理器.Instance != null)
                {
                    音频管理器.Instance.播放强化音效();
                }

                // 播放浮动渐隐提示 UI 动效
                显示奖励浮动提示($"+{amount} 金币！");
            }
            else
            {
                Debug.LogError("[AdReward] 未找到「玩家属性.Instance」单例，金币无法增加！请确保玩家属性已初始化。");
            }
        }

        private void 显示奖励浮动提示(string text)
        {
            if (获得奖励提示UI == null) return;

            if (_提示动效组件 == null)
            {
                _提示动效组件 = 获得奖励提示UI.GetComponent<奖励提示动效>();
                if (_提示动效组件 == null)
                {
                    _提示动效组件 = 获得奖励提示UI.AddComponent<奖励提示动效>();
                }
            }

            if (_提示动效组件 != null)
            {
                _提示动效组件.播放提示(text);
            }
            else
            {
                获得奖励提示UI.SetActive(true);
            }
        }

        private void OnDestroy()
        {
            if (观看广告按钮 != null)
            {
                观看广告按钮.onClick.RemoveListener(点击观看视频领金币);
            }
        }
    }
}
