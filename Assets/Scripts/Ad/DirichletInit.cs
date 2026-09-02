using UnityEngine;
using Dirichlet.Ad;

namespace KnightCharge.Ad
{
    /// <summary>
    /// Dirichlet Ad SDK 全局初始化组件
    /// 建议挂载在游戏第一个加载的场景（如主界面或启动场景）的常驻 GameObject 上
    /// </summary>
    public class DirichletInit : MonoBehaviour
    {
        public static DirichletInit Instance { get; private set; }

        [Header("媒体基本信息配置")]
        [Tooltip("Dirichlet 媒体后台分配的媒体 ID")]
        public long mediaId = 1107182L;

        [Tooltip("媒体名称")]
        public string mediaName = "骑士冲锋";

        [Tooltip("媒体密钥")]
        public string mediaKey = "JJ8QA7OfHQ4ReEER7XaPJ6ceDYEIAObuQXzIzcvS6Ip2X2GYlcEXM24bXn020Fuc";

        [Header("调试与交互设置")]
        [Tooltip("是否开启摇一摇交互")]
        public bool shakeEnabled = true;

        [Tooltip("是否在初始化成功后自动请求 Android 运行时权限（合规要求：必须设为 false，严禁启动时主动申请电话等无关权限）")]
        public bool autoRequestPermissions = false;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);

                // 🎯 合规要求：仅在用户已经同意过隐私政策时才在启动时初始化 SDK
                // 若用户首次打开尚未同意，将由 隐私政策管理器 在用户点击【同意】后调用 InitSdk()
                if (PlayerPrefs.GetInt("PrivacyPolicyAgreed_v1", 0) == 1)
                {
                    InitSdk();
                }
                else
                {
                    Debug.Log("[Dirichlet] 用户尚未同意隐私政策，推迟 SDK 初始化。");
                }
            }
            else
            {
                Destroy(gameObject);
            }
        }

        /// <summary>
        /// 执行 SDK 初始化
        /// </summary>
        public void InitSdk()
        {
            Debug.Log($"[Dirichlet] 开始初始化 SDK... MediaId: {mediaId}, MediaName: {mediaName}");

            var config = new DirichletAdConfig.Builder()
                .WithMediaId(mediaId)
                .WithMediaKey(mediaKey)
                .WithMediaName(mediaName)
                .EnableDebug(Debug.isDebugBuild) // 处于 Debug 模式时打印调试日志，Release 模式自动关闭
                .ShakeEnabled(shakeEnabled)
                .Build();

            DirichletAdSdk.Init(
                config,
                onSuccess: result =>
                {
                    Debug.Log($"[Dirichlet] SDK 初始化成功! 版本: {DirichletAdSdk.GetVersion()}, 信息: {result.Message}");

                    if (autoRequestPermissions)
                    {
                        RequestPermissions();
                    }
                },
                onFailure: error =>
                {
                    Debug.LogError($"[Dirichlet] SDK 初始化失败: Code={error.Code}, Message={error.Message}");
                }
            );
        }

        /// <summary>
        /// 请求 Android 运行时权限（合规注意：游戏无电话/定位等功能，严禁主动弹窗申请）
        /// </summary>
        public void RequestPermissions()
        {
            if (!autoRequestPermissions) return;

#if UNITY_ANDROID && !UNITY_EDITOR
            DirichletAdSdk.RequestPermissionIfNecessary();
#endif
        }
    }
}
