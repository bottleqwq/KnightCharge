using UnityEngine;
using UnityEngine.UI;

public class 声音控制 : MonoBehaviour
{
    [Header("UI 组件")]
    [SerializeField] private Image buttonImage;      // 按钮的 Image 组件
    [SerializeField] private Sprite soundOnSprite;    // 声音开启的图标
    [SerializeField] private Sprite soundOffSprite;   // 声音关闭的图标

    private bool isMuted = false;

    private void Start()
    {
        // 1. 从本地读取声音状态（0 表示开启，1 表示静音，默认开启）
        isMuted = PlayerPrefs.GetInt("Muted", 0) == 1;

        // 2. 初始化声音和图标状态
        ApplySoundState();
    }

    // 按钮点击时调用的方法
    public void ToggleSound()
    {
        // 1. 切换状态
        isMuted = !isMuted;

        // 2. 保存设置到本地
        PlayerPrefs.SetInt("Muted", isMuted ? 1 : 0);
        PlayerPrefs.Save();

        // 3. 应用状态
        ApplySoundState();
    }

    private void ApplySoundState()
    {
        if (isMuted)
        {
            AudioListener.volume = 0f;            // 全局静音
            if (buttonImage != null) buttonImage.sprite = soundOffSprite; // 切换为静音图标
        }
        else
        {
            AudioListener.volume = 1f;            // 全局恢复音量
            if (buttonImage != null) buttonImage.sprite = soundOnSprite;  // 切换为有声音标
            音频管理器.Instance.播放按钮点击音效();
        }
    }
}