using UnityEngine;

public class 护盾 : MonoBehaviour
{
    public float 飞行速度 = 10f;
    public float 飞行加速度 = 3f;
    public float 存在时间 = 10f;
    public float 闪烁倒计时 = 3f;
    public float 闪烁频率 = 15f;
    private Transform 玩家位置;
    private float spawnTime;
    private bool isMagnetized = false;
    private SpriteRenderer sr; // 用于控制颜色

    void Start()
    {
        spawnTime = Time.time;
        sr = GetComponent<SpriteRenderer>();
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null) 玩家位置 = playerObj.transform;
    }

    void Update()
    {
        if (玩家位置 == null) return;

        // 计算存活了多久
        float timeAlive = Time.time - spawnTime;
        // 计算还剩多少时间消失
        float timeRemaining = 存在时间 - timeAlive;

        // 1. 检查是否需要销毁
        if (!isMagnetized && timeRemaining <= 0)
        {
            Destroy(gameObject);
            return;
        }

        // 2. 如果已经被吸附，恢复颜色并飞行
        if (isMagnetized)
        {
            // 确保被吸附时是完全可见的（防止飞的过程中是半透明的）
            ResetColor();
            FlyToPlayer();
            return;
        }

        // 3. 处理闪烁逻辑 (未被吸附 且 剩余时间小于设定值)
        if (timeRemaining <= 闪烁倒计时)
        {
            DoFlash();
        }

        // 缓冲期
        if (timeAlive < 0.5f) return;

        // 4. 检查吸附逻辑
        float distance = Vector2.Distance(transform.position, 玩家位置.position);
        if (玩家属性.Instance != null && distance < 玩家属性.Instance.拾取范围)
        {
            isMagnetized = true;
        }
    }
    // 执行闪烁效果
    void DoFlash()
    {
        if (sr != null)
        {
            // 使用 Sin 函数在 0.2 到 1.0 之间循环透明度
            // Time.time * 闪烁频率 决定变化的快慢
            float alpha = 0.2f + Mathf.Abs(Mathf.Sin(Time.time * 闪烁频率)) * 0.8f;

            Color c = sr.color;
            c.a = alpha;
            sr.color = c;
        }
    }
    // 重置颜色为完全不透明
    void ResetColor()
    {
        if (sr != null && sr.color.a < 1f)
        {
            Color c = sr.color;
            c.a = 1f;
            sr.color = c;
        }
    }
    void FlyToPlayer()
    {
        transform.position = Vector2.MoveTowards(transform.position, 玩家位置.position, 飞行速度 * Time.deltaTime);
        飞行速度 += 飞行加速度 * Time.deltaTime;

        if (Vector2.Distance(transform.position, 玩家位置.position) < 0.2f)
        {
            收集();
        }
    }

    private void 收集()
    {
        玩家控制器 player = 玩家位置.GetComponent<玩家控制器>();
        if (player != null)
        {
            player.获得临时护盾(玩家属性.Instance.护盾时长);
        }

        音频管理器.Instance.播放金币音效();
        Destroy(gameObject);
    }
}