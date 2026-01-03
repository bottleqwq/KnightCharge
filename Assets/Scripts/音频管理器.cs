using UnityEngine;

public class 音频管理器 : MonoBehaviour
{
    public static 音频管理器 Instance;

    private AudioSource audioSource;
    public AudioClip 强化音效;
    public AudioClip 命中音效;
    public AudioClip 冲刺音效;
    public AudioClip 金币音效;
    public AudioClip 金币掉落音效;

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
        audioSource = GetComponent<AudioSource>();

    }

    public void 播放命中音效()
    {
        audioSource.PlayOneShot(命中音效);
    }
    public void 播放冲刺音效()
    {
        audioSource.PlayOneShot(冲刺音效);
    }
    public void 播放金币音效()
    {
        audioSource.PlayOneShot(金币音效);
    }
    public void 播放金币掉落音效()
    {
        audioSource.PlayOneShot(金币掉落音效);
    }
    public void 播放强化音效()
    {
        audioSource.PlayOneShot(强化音效);
    }
}
