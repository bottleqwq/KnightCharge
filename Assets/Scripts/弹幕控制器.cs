using UnityEngine;

public class 弹幕控制器 : MonoBehaviour
{
    [Header("弹幕属性")]
    public float 移动速度 = 5f;
    public int 伤害 = 1;
    public float 存活时间 = 5f; // 弹幕存在时间，避免一直存在

    private Vector2 移动方向;
    private Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        
        // 自动销毁，避免弹幕一直存在
        Destroy(gameObject, 存活时间);
    }

    /// <summary>
    /// 初始化弹幕：设置移动方向
    /// </summary>
    public void 初始化(Vector2 direction)
    {
        移动方向 = direction.normalized;
        
        // 设置弹幕朝向
        if (移动方向 != Vector2.zero)
        {
            float angle = Mathf.Atan2(移动方向.y, 移动方向.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        }
    }

    void Update()
    {
        // 移动弹幕
        if (rb != null)
        {
            rb.velocity = 移动方向 * 移动速度;
        }
        else
        {
            // 如果没有Rigidbody2D，使用Transform移动
            transform.position += (Vector3)(移动方向 * 移动速度 * Time.deltaTime);
        }
    }

    // 碰撞检测：碰到玩家
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            玩家控制器 playerScript = other.GetComponent<玩家控制器>();
            if (playerScript != null)
            {
                // 对玩家造成伤害
                playerScript.TakeDamage(伤害);
                
                // 销毁弹幕
                Destroy(gameObject);
            }
        }
    }

    // 碰撞检测：碰到墙壁或其他障碍物
    void OnCollisionEnter2D(Collision2D collision)
    {
        // 如果碰到非玩家的物体，销毁弹幕
        if (!collision.gameObject.CompareTag("Player"))
        {
            Destroy(gameObject);
        }
    }
}

