using UnityEngine;

public class 血瓶 : MonoBehaviour
{
    public float 飞行速度 = 3f;
    public float 飞行加速度 = 3f;
    public float 存在时间 = 10f; // 10秒后消失

    private Transform 玩家位置;
    private float spawnTime;
    private bool isMagnetized = false;

    void Start()
    {
        spawnTime = Time.time;
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null) 玩家位置 = playerObj.transform;
    }

    void Update()
    {
        if (玩家位置 == null) return;

        // 超时销毁
        if (!isMagnetized && Time.time > spawnTime + 存在时间)
        {
            Destroy(gameObject);
            return;
        }

        if (isMagnetized)
        {
            FlyToPlayer();
            return;
        }

        if (Time.time < spawnTime + 0.5f) return;

        // 吸附检测
        float distance = Vector2.Distance(transform.position, 玩家位置.position);
        if (玩家属性.Instance != null && distance < 玩家属性.Instance.拾取范围)
        {
            isMagnetized = true;
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
        // 获取玩家脚本并回血
        玩家控制器 player = 玩家位置.GetComponent<玩家控制器>();
        if (player != null)
        {
            player.恢复生命(玩家属性.Instance.血瓶恢复值);
        }

        // 如果有特定的音效方法，请替换下面这一行
        音频管理器.Instance.播放金币音效();
        Destroy(gameObject);
    }
}