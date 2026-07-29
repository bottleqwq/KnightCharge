using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;

public class 声音控制 : MonoBehaviour
{
    // 定义枚举来区分类型
    public enum AudioType { Music, SFX }

    [Header("设置类型")]
    [SerializeField] private AudioType type;

    [Header("Audio Mixer 配置")]
    [SerializeField] private AudioMixer audioMixer; // 拖入刚才创建的 MainMixer
    [SerializeField] private string mixerParameterName; // 填入 "MusicVol" 或 "SFXVol"

    [Header("UI 组件")]
    [SerializeField] private Image buttonImage;      // 按钮的 Image 组件
    [SerializeField] private Sprite soundOnSprite;    // 开启图标
    [SerializeField] private Sprite soundOffSprite;   // 关闭图标

    private bool isMuted = false;
    private string prefsKey;

    private void Start()
    {
        // 根据类型设置本地保存的 Key 值
        prefsKey = (type == AudioType.Music) ? "MusicMuted" : "SFXMuted";

        // 读取本地保存的状态
        isMuted = PlayerPrefs.GetInt(prefsKey, 0) == 1;

        // 应用声音状态
        ApplySoundState();
    }

    public void ToggleSound()
    {
        isMuted = !isMuted;

        // 保存状态
        PlayerPrefs.SetInt(prefsKey, isMuted ? 1 : 0);
        PlayerPrefs.Save();

        // 应用状态
        ApplySoundState();
    }

    private void ApplySoundState()
    {
        // 在 Audio Mixer 中，音量大小是以分贝（dB）计算的。
        // 0 分贝代表正常音量（无衰减），-80 分贝代表完全静音。
        float targetVolume = isMuted ? -80f : 0f;

        // 设置 Mixer 对应参数的值
        if (audioMixer != null && !string.IsNullOrEmpty(mixerParameterName))
        {
            audioMixer.SetFloat(mixerParameterName, targetVolume);
        }

        // 更新 UI 图标
        if (buttonImage != null)
        {
            buttonImage.sprite = isMuted ? soundOffSprite : soundOnSprite;
        }
    }
}