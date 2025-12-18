using UnityEngine;

public class 金币 : MonoBehaviour
{
    public int 金币价值 = 1;
    public float 飞行速度 = 3f;
    public float 飞行加速度 = 3f;
    private Transform 玩家位置;
    private float spawnTime; //缓冲时间
    private bool isMagnetized = false; // 标记是否已经被吸附

    void Start()
    {
        spawnTime = Time.time;

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            玩家位置 = playerObj.transform;
        }
    }
    void Update()
    {
        // 安全检查：如果玩家死了或单例不存在，就不跑逻辑
        if (玩家位置 == null) return;

        // 如果已经被吸附，直接飞
        if (isMagnetized)
        {
            FlyToPlayer();
            return;
        }

        // 缓冲期（例如生成后的0.5秒内不吸附，让金币先散开）
        if (Time.time < spawnTime + 0.5f) return;

        // 核心修改：直接通过单例访问 拾取范围
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

        // 距离极近时直接拾取，防止穿模
        if (Vector2.Distance(transform.position, 玩家位置.position) < 0.2f)
        {
            收集();
        }
    }
    private void 收集()
    {
        玩家属性.Instance.增加金币(金币价值);
        音频管理器.Instance.播放金币音效();
        Destroy(gameObject);
    }
}
