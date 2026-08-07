using TMPro;
using UnityEngine;

public class 难度等级UI : MonoBehaviour
{
    private TextMeshProUGUI 难度文本;
    private 敌人生成器 生成器;

    private void Start()
    {
        难度文本 = GetComponent<TextMeshProUGUI>();
        生成器 = FindObjectOfType<敌人生成器>();
    }

    private void Update()
    {
        if (难度文本 == null) return;

        int 难度等级 = 生成器 != null ? 生成器.获取当前难度等级() : 0;
        难度文本.text = $"难度: {难度等级}";
    }
}
