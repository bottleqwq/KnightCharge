using UnityEngine;
using System.Collections;

public class 敌人控制器 : MonoBehaviour
{
    [Header("敌人属性")]
    public float 移动速度 = 2f;
    public float 最大生命 = 50f;
    public int 伤害 = 1;

    [Header("受击反馈")]
    public float 击退力度 = 10f;
    public float 受击变色时间 = 0.1f;
    public float 受击僵直时间 = 0.5f; // 受到伤害后的僵直时间
    public float 攻击僵直时间 = 0.3f; // 对玩家造成伤害后的僵直时间

    private Transform playerTransform;
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private float 当前生命;
    private bool isKnockedBack = false; // 是否处于被击退状态
    private bool isStunned = false; // 是否处于僵直状态

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        当前生命 = 最大生命;

        // 自动寻找场景里的 Player 标签物体
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            playerTransform = playerObj.transform;
        }
    }

    void Update()
    {
        // 如果没有被击退、没有僵直，且玩家存在，就向玩家移动
        if (!isKnockedBack && !isStunned && playerTransform != null)
        {
            MoveTowardsPlayer();
        }
    }

    void MoveTowardsPlayer()
    {
        // 计算方向
        Vector2 direction = (playerTransform.position - transform.position).normalized;
        // 移动位置 (也可以用 rb.velocity，但 MoveTowards 对简单AI更稳定)
        transform.position = Vector2.MoveTowards(transform.position, playerTransform.position, 移动速度 * Time.deltaTime);
    }

    // 1. 碰撞检测：碰到玩家身体
    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            玩家控制器 playerScript = collision.gameObject.GetComponent<玩家控制器>();
            if (playerScript != null)
            {
                // 对玩家造成伤害
                playerScript.TakeDamage(伤害);

                // 触发攻击僵直
                StartCoroutine(StunRoutine(攻击僵直时间));

                // 可选：碰到玩家后自己也后退一点，防止粘在玩家身上一直扣血
                Vector2 pushBack = (transform.position - collision.transform.position).normalized;
                rb.AddForce(pushBack * 5f, ForceMode2D.Impulse);
            }
        }
    }

    // 2. 触发检测：碰到剑 (Sword是Trigger，所以用 OnTriggerEnter2D)
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Sword"))
        {
            玩家控制器 playerScript = other.GetComponentInParent<玩家控制器>();

            if (playerScript != null)
            {
                if (playerScript.isDashing)
                {
                    float chargePercent = playerScript.上次蓄力百分比;
                    // 从玩家控制器获取伤害值
                    float damage = playerScript.CalculateDamage();
                    Vector2 knockbackDir = playerScript.剑的旋转轴.right;

                    // 执行伤害
                    TakeDamage(damage, knockbackDir, chargePercent);
                }
            }
        }
    }

    void TakeDamage(float damage, Vector2 knockbackDir, float chargePercent)
    {
        当前生命 -= damage;
        Debug.Log($"敌人受到伤害: {damage}, 剩余血量: {当前生命 }");

        // 触发受击僵直
        StartCoroutine(StunRoutine(受击僵直时间));

        // 击退逻辑
        StartCoroutine(KnockbackRoutine(knockbackDir, chargePercent));

        // 变色反馈
        StartCoroutine(FlashColor());

        if (当前生命 <= 0)
        {
            Die();
        }
    }

    // 击退协程
    IEnumerator KnockbackRoutine(Vector2 dir, float chargePower)
    {
        isKnockedBack = true;

        // 蓄力越久，击退力度越大
        float finalForce = 击退力度 * (0.5f + chargePower); // 0.5倍到1.5倍力度

        rb.velocity = Vector2.zero; // 清空当前速度
        rb.AddForce(dir * finalForce, ForceMode2D.Impulse);

        // 击退持续时间（这期间敌人不会向玩家移动）
        yield return new WaitForSeconds(0.3f);

        isKnockedBack = false;
    }

    IEnumerator FlashColor()
    {
        spriteRenderer.color = Color.red;
        yield return new WaitForSeconds(受击变色时间);
        spriteRenderer.color = Color.white;
    }

    // 僵直协程
    IEnumerator StunRoutine(float stunDuration)
    {
        isStunned = true;
        
        // 僵直期间停止移动（速度清零）
        if (rb != null)
        {
            rb.velocity = Vector2.zero;
        }
        
        yield return new WaitForSeconds(stunDuration);
        
        isStunned = false;
    }

    void Die()
    {
        // 这里可以播放死亡动画或粒子特效
        Destroy(gameObject);
    }
}
