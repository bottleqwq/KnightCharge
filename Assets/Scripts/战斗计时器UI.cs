using TMPro;
using UnityEngine;

public class 战斗计时器UI : MonoBehaviour
{
    private TextMeshProUGUI 计时文本;

    private void Start()
    {
        计时文本 = GetComponent<TextMeshProUGUI>();
    }

    private void Update()
    {
        if (计时文本 == null) return;

        int 总秒数 = Mathf.FloorToInt(Time.timeSinceLevelLoad);
        int 分钟 = 总秒数 / 60;
        int 秒 = 总秒数 % 60;
        计时文本.text = $"时间: {分钟:00}:{秒:00}";
    }
}
