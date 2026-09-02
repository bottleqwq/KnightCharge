using UnityEngine;
using UnityEngine.SceneManagement;

public class 主界面菜单 : MonoBehaviour
{
    // 引用我们的游戏说明界面
    public GameObject 游戏说明界面;


    // 进入游戏
    public void 进入游戏()
    {
        SceneManager.LoadScene("备战界面");
        音频管理器.Instance.播放按钮点击音效();
    }
    //游戏说明
    public void 游戏说明()
    {
        游戏说明界面.SetActive(!游戏说明界面.activeSelf);
        音频管理器.Instance.播放按钮点击音效();
    }

    // 隐私政策
    public void 查看隐私政策()
    {
        if (隐私政策管理器.Instance != null)
        {
            隐私政策管理器.Instance.ShowPrivacyPolicyManually();
        }
        else
        {
            Debug.LogWarning("未找到 隐私政策管理器 实例！");
        }

        if (音频管理器.Instance != null)
        {
            音频管理器.Instance.播放按钮点击音效();
        }
    }

    // 退出游戏
    public void 退出游戏()
    {
        Debug.Log("Quit Game..."); // 编辑器里看不到退出效果，打印一条日志验证
        Application.Quit();
    }

}
