using UnityEngine;

public class 音频管理器 : MonoBehaviour
{
    public static 音频管理器 Instance;

    private AudioSource audioSource;
    public AudioClip 命中音效;

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
}
