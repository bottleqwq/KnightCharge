using UnityEngine;

public class 法阵行为 : MonoBehaviour
{
    [Header("组件引用")]
    public Transform 缩圈Transform; // 拖入那个会缩小的圆环物体
    public SpriteRenderer 底色Sprite; // 拖入底下的红色圆

    private float 总时间;
    private float 计时器;
    private bool 开始运行 = false;
    private float 初始大小;

    // 初始化函数，由法师脚本调用
    public void 初始化(float 持续时间, float 半径)
    {
        总时间 = 持续时间;
        计时器 = 0f;
        初始大小 = 半径 * 2; // 半径转直径

        // 设置底色大小
        transform.localScale = Vector3.one * 初始大小;

        // 确保缩圈一开始也是最大
        if (缩圈Transform != null)
        {
            // 因为缩圈是子物体，且父物体已经放大了，所以子物体保持 Scale 1 就是满大小
            缩圈Transform.localScale = Vector3.one;
        }

        开始运行 = true;
    }

    void Update()
    {
        if (!开始运行) return;

        计时器 += Time.deltaTime;

        // 计算进度 (0 到 1)
        float progress = Mathf.Clamp01(计时器 / 总时间);

        // 如果有缩圈物体，执行“由外向内”的缩小动画
        if (缩圈Transform != null)
        {
            // Lerp(1, 0, progress) 表示从 1 变到 0
            float currentScale = Mathf.Lerp(1f, 0f, progress);
            缩圈Transform.localScale = Vector3.one * currentScale;
        }

        // 可选：让底色稍微闪烁一下，增加紧张感
        if (底色Sprite != null)
        {
            // 透明度随着时间越来越高
            Color c = 底色Sprite.color;
            c.a = Mathf.Lerp(0.3f, 0.6f, progress);
            底色Sprite.color = c;
        }
    }
}