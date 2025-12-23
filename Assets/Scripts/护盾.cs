using UnityEngine;

public class 护盾 : MonoBehaviour
{
    public float 无敌持续时间 = 5f; // 拾取后无敌5秒
    public float 飞行速度 = 3f;
    public float 飞行加速度 = 3f;
    public float 存在时间 = 10f;

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
        玩家控制器 player = 玩家位置.GetComponent<玩家控制器>();
        if (player != null)
        {
            player.获得临时护盾(无敌持续时间);
        }

        音频管理器.Instance.播放金币音效();
        Destroy(gameObject);
    }
}