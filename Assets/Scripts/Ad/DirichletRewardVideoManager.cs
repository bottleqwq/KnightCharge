using System;
using System.Collections;
using UnityEngine;
using Dirichlet.Ad;

namespace KnightCharge.Ad
{
    /// <summary>
    /// Dirichlet 激励视频广告单例管理器
    /// 封装激励视频的加载、展示、奖励核验、事件监听与资源释放
    /// </summary>
    public class DirichletRewardVideoManager : MonoBehaviour
    {
        public static DirichletRewardVideoManager Instance { get; private set; }

        [Header("广告位配置")]
        [Tooltip("Dirichlet 激励视频广告位 ID（当前接入位: 1063658）")]
        public long spaceId = 1063658L;

        [Header("编辑器测试配置")]
        [Tooltip("在 Unity Editor 中自动模拟广告播放成功并发放奖励（方便开发调试）")]
        public bool simulateSuccessInEditor = true;

        [Tooltip("在 Unity Editor 中模拟播放的时长（秒）")]
        public float editorSimulateDuration = 0.5f;

        private DirichletAdNative _adNative;
        private DirichletRewardVideoAd _currentRewardAd;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                EnsureNativeAd();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void EnsureNativeAd()
        {
            if (_adNative == null)
            {
                try
                {
                    _adNative = DirichletAdManager.CreateAdNative();
                }
                catch (Exception ex)
                {
                    Debug.LogWarning($"[Dirichlet] CreateAdNative 提示: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// 展示激励视频广告
        /// </summary>
        /// <param name="onRewardSuccess">奖励核验成功回调（仅在 args.IsVerified == true 时触发）</param>
        /// <param name="onAdClosed">广告关闭回调（用于恢复游戏背景音效、交互等）</param>
        /// <param name="onAdFailed">广告加载或展示失败回调 (errorCode, errorMessage)</param>
        /// <param name="targetSpaceId">可选：自定义广告位 ID（若不传则使用默认 spaceId: 1063658）</param>
        public void ShowRewardVideo(Action onRewardSuccess, Action onAdClosed = null, Action<string, string> onAdFailed = null, long? targetSpaceId = null)
        {
#if UNITY_EDITOR
            if (simulateSuccessInEditor)
            {
                StartCoroutine(SimulateEditorRewardVideo(onRewardSuccess, onAdClosed));
                return;
            }
#endif

            if (!DirichletAdSdk.IsInitialized)
            {
                Debug.LogWarning("[Dirichlet] SDK 尚未初始化完成，无法请求激励视频！");
                onAdFailed?.Invoke("-1", "SDK 尚未初始化完成");
                return;
            }

            EnsureNativeAd();

            if (_adNative == null)
            {
                Debug.LogError("[Dirichlet] DirichletAdNative 未能成功创建！");
                onAdFailed?.Invoke("-2", "AdNative is null");
                return;
            }

            // 销毁上一次未释放的广告对象
            DestroyCurrentAd();

            long activeSpaceId = targetSpaceId ?? spaceId;
            var request = new DirichletAdRequest.Builder()
                .WithSpaceId(activeSpaceId)
                .Build();

            Debug.Log($"[Dirichlet] 开始加载激励视频广告: SpaceId={activeSpaceId}");

            _adNative.LoadRewardVideoAd(
                request,
                onLoaded: ad =>
                {
                    _currentRewardAd = ad;
                    bool rewardVerified = false;

                    // 设置交互监听
                    _currentRewardAd.SetInteractionListener(new RewardAdInteractionListener(
                        onShow: () => Debug.Log("[Dirichlet] 激励视频开始展示"),
                        onClick: () => Debug.Log("[Dirichlet] 激励视频被点击"),
                        onClose: () =>
                        {
                            Debug.Log("[Dirichlet] 激励视频已关闭");
                            onAdClosed?.Invoke();
                            DestroyCurrentAd();
                        },
                        onReward: args =>
                        {
                            Debug.Log($"[Dirichlet] 激励视频核验结果: IsVerified={args.IsVerified}, Reward={args.RewardAmount} {args.RewardName}");
                            if (args.IsVerified && !rewardVerified)
                            {
                                rewardVerified = true;
                                onRewardSuccess?.Invoke();
                            }
                        }
                    ));

                    // 展示广告
                    bool showResult = _currentRewardAd.Show();
                    Debug.Log($"[Dirichlet] 激励视频 Show() 调用结果: {showResult}");
                },
                onFailure: error =>
                {
                    Debug.LogError($"[Dirichlet] 激励视频加载失败: [{error.Code}] {error.Message}");
                    onAdFailed?.Invoke(error.Code, error.Message);
                    DestroyCurrentAd();
                }
            );
        }

#if UNITY_EDITOR
        private IEnumerator SimulateEditorRewardVideo(Action onRewardSuccess, Action onAdClosed)
        {
            Debug.Log("<color=yellow>[Dirichlet Editor 模拟]</color> 开始模拟播放激励视频...");
            if (editorSimulateDuration > 0)
            {
                yield return new WaitForSecondsRealtime(editorSimulateDuration);
            }

            Debug.Log("<color=green>[Dirichlet Editor 模拟]</color> 模拟激励视频播放完成，核验成功！");
            onRewardSuccess?.Invoke();

            yield return null;
            Debug.Log("<color=yellow>[Dirichlet Editor 模拟]</color> 模拟关闭广告窗口。");
            onAdClosed?.Invoke();
        }
#endif

        private class RewardAdInteractionListener : IDirichletRewardAdInteractionListener
        {
            private readonly Action _onShow;
            private readonly Action _onClick;
            private readonly Action _onClose;
            private readonly Action<DirichletRewardVerificationEventArgs> _onReward;

            public RewardAdInteractionListener(
                Action onShow,
                Action onClick,
                Action onClose,
                Action<DirichletRewardVerificationEventArgs> onReward)
            {
                _onShow = onShow;
                _onClick = onClick;
                _onClose = onClose;
                _onReward = onReward;
            }

            public void OnAdShow() => _onShow?.Invoke();
            public void OnAdClick() => _onClick?.Invoke();
            public void OnAdClose() => _onClose?.Invoke();
            public void OnRewardVerify(DirichletRewardVerificationEventArgs args) => _onReward?.Invoke(args);
        }

        private void DestroyCurrentAd()
        {
            if (_currentRewardAd != null)
            {
                try
                {
                    _currentRewardAd.Destroy();
                }
                catch (Exception ex)
                {
                    Debug.LogWarning($"[Dirichlet] Destroy 异常: {ex.Message}");
                }
                _currentRewardAd = null;
            }
        }

        private void OnDestroy()
        {
            DestroyCurrentAd();
        }
    }
}
