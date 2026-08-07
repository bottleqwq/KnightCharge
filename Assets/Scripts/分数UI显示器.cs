using TMPro;
using UnityEngine;

public class 分数UI显示器 : MonoBehaviour
{
    [Tooltip("勾选后显示历史最高得分，否则显示当前得分")]
    public bool 显示历史最高分 = false;

    private void Start()
    {
        TextMeshProUGUI myText = GetComponent<TextMeshProUGUI>();

        if (分数管理器.Instance != null)
        {
            if (显示历史最高分)
            {
                分数管理器.Instance.注册最高得分文本(myText);
            }
            else
            {
                分数管理器.Instance.注册当前得分文本(myText);
            }
        }
        else
        {
            Debug.LogWarning("场景里没找到 分数管理器，UI 无法显示分数！");
        }
    }
}
