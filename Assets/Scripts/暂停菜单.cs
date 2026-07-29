using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

public class 暂停菜单 : MonoBehaviour
{
    // 定义一个变量来检测当前是否暂停
    public static bool GameIsPaused = false;

    // 引用我们的暂停菜单UI面板
    public GameObject pauseMenuUI;

    void Start()
    {
        pauseMenuUI.SetActive(false);
    }
    void Update()
    {
        // 检测是否按下了 ESC 键
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (GameIsPaused)
            {
                返回();
            }
            else
            {
                暂停();
            }
        }
    }


    public void 返回()
    {
        音频管理器.Instance.播放按钮点击音效();
        pauseMenuUI.SetActive(false); // 隐藏暂停菜单
        Time.timeScale = 1f;          // 恢复游戏时间（1是正常速度）
        GameIsPaused = false;
    }

    public void 暂停()
    {
        音频管理器.Instance.播放按钮点击音效();
        pauseMenuUI.SetActive(true);  // 显示暂停菜单
        Time.timeScale = 0f;          // 冻结游戏时间（0是静止）
        GameIsPaused = true;
    }

    public void 退出()
    {
        音频管理器.Instance.播放按钮点击音效();
        // 恢复时间！非常重要，否则下一局游戏开始时时间还是静止的
        Time.timeScale = 1f;
        GameIsPaused = false;

        // "MainMenu" 需替换成你主界面场景的准确名字
        SceneManager.LoadScene("备战界面");
    }

}

