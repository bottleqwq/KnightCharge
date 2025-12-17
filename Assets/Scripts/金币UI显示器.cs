using TMPro;
using UnityEngine;

public class 金币UI显示器 : MonoBehaviour
{
    private void Start()
    {
        // 1. 获取自身组件
        TextMeshProUGUI myText = GetComponent<TextMeshProUGUI>();

        // 2. 找到单例，并把把自己“注册”进去
        if (玩家属性.Instance != null)
        {
            玩家属性.Instance.注册金币显示文本(myText);
        }
        else
        {
            Debug.LogWarning("场景里没找到 玩家属性，UI 无法显示金币！");
        }
    }
}
