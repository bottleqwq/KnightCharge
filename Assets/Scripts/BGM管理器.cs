using UnityEngine;

public class BGMManager : MonoBehaviour
{
    // 静态变量，用来保存当前存在的那个音乐播放器
    public static BGMManager instance;

    void Awake()
    {
        // 如果系统里已经有一个 BGMManager 了（比如从场景2回到场景1时）
        if (instance != null)
        {
            // 销毁当前这个新的（避免出现两个背景音乐同时放）
            Destroy(gameObject);
        }
        else
        {
            // 如果是第一个，就把它标记为“老大”
            instance = this;
            // 告诉 Unity：切换场景时，不要销毁这个物体
            DontDestroyOnLoad(gameObject);
        }
    }
}
