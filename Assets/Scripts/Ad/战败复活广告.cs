using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace KnightCharge.Ad
{
    /// <summary>
    /// 阵亡面板「看视频复活」业务逻辑组件
    /// 挂载在阵亡面板的复活按钮或阵亡面板 Canvas 节点上
    /// </summary>
    public class 战败复活广告 : MonoBehaviour
    {
        [Header("UI 绑定")]
        [Tooltip("复活按钮（若为空则自动获取自身 Button 组件）")]
        public Button 复活按钮;

        [Tooltip("按钮上的说明文字（例如 '看视频复活'）")]
        public TextMeshProUGUI 按钮文本;

        [Header("玩家引用")]
        [Tooltip("玩家控制器（若为空则自动在场景中查找）")]
        public 玩家控制器 玩家;

        [Header("复活限制")]
        [Tooltip("每局最大复活次数（<=0 表示不限制次数）")]
        public int 每局最大复活次数 = 1;

        private int _当前已复活次数 = 0;

        private void Awake()
        {
            if (复活按钮 == null)
            {
                复活按钮 = GetComponent<Button>();
            }

            if (玩家 == null)
            {
                玩家 = FindObjectOfType<玩家控制器>();
            }
        }

        private void OnEnable()
        {
            // 当阵亡面板打开时，检测是否还能复活
            if (每局最大复活次数 > 0 && _当前已复活次数 >= 每局最大复活次数)
            {
                if (复活按钮 != null)
                {
                    复活按钮.gameObject.SetActive(false);
                }
            }
            else
            {
                if (复活按钮 != null)
                {
                    复活按钮.gameObject.SetActive(true);
                    SetButtonInteractable(true);
                }
            }
        }

        private void Start()
        {
            if (复活按钮 != null)
            {
                复活按钮.onClick.AddListener(点击看视频复活);
            }

            if (按钮文本 != null)
            {
                按钮文本.text = "看视频复活";
            }
        }

        /// <summary>
        /// 点击按钮响应方法
        /// </summary>
        public void 点击看视频复活()
        {
            if (每局最大复活次数 > 0 && _当前已复活次数 >= 每局最大复活次数)
            {
                Debug.LogWarning("[战败复活广告] 本局复活次数已用尽！");
                return;
            }

            Debug.Log("[战败复活广告] 用户点击看视频复活");

            // 播放点击音效
            if (音频管理器.Instance != null)
            {
                音频管理器.Instance.播放按钮点击音效();
            }

            // 防止重复点击，暂时禁用按钮
            SetButtonInteractable(false);

            // 设置超时保护，10秒内若未收到任何回调自动恢复按钮
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
                    _当前已复活次数++;
                    Debug.Log($"<color=green>[战败复活广告] 激励视频观看成功！执行复活（第 {_当前已复活次数} 次）</color>");

                    if (玩家 == null)
                    {
                        玩家 = FindObjectOfType<玩家控制器>();
                    }

                    if (玩家 != null)
                    {
                        玩家.执行复活();
                    }
                    else
                    {
                        Debug.LogError("[战败复活广告] 未找到玩家控制器实例，无法执行复活！");
                    }
                },
                onAdClosed: () =>
                {
                    Debug.Log("[战败复活广告] 广告已关闭，恢复按钮交互");
                    CancelInvoke(nameof(超时恢复按钮));
                    SetButtonInteractable(true);
                },
                onAdFailed: (errorCode, errorMessage) =>
                {
                    Debug.LogWarning($"[战败复活广告] 广告加载或展示失败: [{errorCode}] {errorMessage}");
                    CancelInvoke(nameof(超时恢复按钮));
                    SetButtonInteractable(true);
                }
            );
        }

        private void 超时恢复按钮()
        {
            Debug.LogWarning("[战败复活广告] 广告回调等待超时，自动恢复按钮交互");
            SetButtonInteractable(true);
        }

        private void SetButtonInteractable(bool state)
        {
            if (复活按钮 != null)
            {
                复活按钮.interactable = state;
            }
        }

        private void OnDestroy()
        {
            if (复活按钮 != null)
            {
                复活按钮.onClick.RemoveListener(点击看视频复活);
            }
        }
    }
}
