using UnityEngine;
using System.Collections;

public class 远程敌人控制器 : MonoBehaviour
{
    [Header("敌人属性")]
    public float 移动速度 = 2f;
    public float 最大生命 = 50f;
    public int 伤害 = 1;

    [Header("远程攻击设置")]
    public float 攻击距离 = 5f; // 距离玩家多近时开始攻击
    public float 发射间隔 = 2f; // 发射弹幕的间隔时间
    public GameObject 弹幕预制体; // 弹幕预制体，需要在Inspector中设置

    [Header("受击反馈")]
    public float 受击变色时间 = 0.1f;
    public float 受击僵直时间 = 0.5f; // 受到伤害后的僵直时间

    private Transform playerTransform;
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private float 当前生命;
    private bool isKnockedBack = false; // 是否处于被击退状态
    private bool isStunned = false; // 是否处于僵直状态
    private bool isInAttackRange = false; // 是否在攻击范围内
    private float 上次发射时间 = 0f; // 上次发射弹幕的时间

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

        // 检查弹幕预制体是否设置
        if (弹幕预制体 == null)
        {
            Debug.LogWarning("远程敌人控制器：弹幕预制体未设置！");
        }
    }

    void Update()
    {
        if (playerTransform == null) return;

        // 计算到玩家的距离
        float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);
        isInAttackRange = distanceToPlayer <= 攻击距离;

        // 如果没有被击退、没有僵直，且玩家存在
        if (!isKnockedBack && !isStunned && playerTransform != null)
        {
            if (isInAttackRange)
            {
                // 在攻击范围内：停止移动，发射弹幕
                StopMoving();
                TryShoot();
            }
            else
            {
                // 不在攻击范围内：向玩家移动
                MoveTowardsPlayer();
            }
        }
    }

    void MoveTowardsPlayer()
    {
        // 计算方向并移动
        Vector2 direction = (playerTransform.position - transform.position).normalized;
        transform.position = Vector2.MoveTowards(transform.position, playerTransform.position, 移动速度 * Time.deltaTime);
    }

    void StopMoving()
    {
        // 停止移动（速度清零）
        if (rb != null)
        {
            rb.velocity = Vector2.zero;
        }
    }

    void TryShoot()
    {
        // 检查是否可以发射（距离上次发射的时间间隔）
        if (Time.time - 上次发射时间 >= 发射间隔)
        {
            Shoot();
            上次发射时间 = Time.time;
        }
    }

    void Shoot()
    {
        if (弹幕预制体 == null || playerTransform == null) return;

        // 计算朝向玩家的方向
        Vector2 direction = (playerTransform.position - transform.position).normalized;

        // 实例化弹幕
        GameObject bullet = Instantiate(弹幕预制体, transform.position, Quaternion.identity);
        
        // 初始化弹幕
        弹幕控制器 bulletController = bullet.GetComponent<弹幕控制器>();
        if (bulletController != null)
        {
            bulletController.初始化(direction);
        }
        else
        {
            Debug.LogWarning("远程敌人控制器：弹幕预制体缺少弹幕控制器组件！");
        }
    }

    // 碰撞检测：碰到剑 (Sword是Trigger，所以用 OnTriggerEnter2D)
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
                    float knockbackForce = playerScript.击退力度; // 从玩家控制器获取击退力度

                    // 执行伤害
                    TakeDamage(damage, knockbackDir, chargePercent, knockbackForce);
                }
            }
        }
    }

    void TakeDamage(float damage, Vector2 knockbackDir, float chargePercent, float knockbackForce)
    {
        当前生命 -= damage;
        Debug.Log($"远程敌人受到伤害: {damage}, 剩余血量: {当前生命}");

        // 触发受击僵直
        StartCoroutine(StunRoutine(受击僵直时间));

        // 击退逻辑（使用从玩家控制器获取的击退力度）
        StartCoroutine(KnockbackRoutine(knockbackDir, chargePercent, knockbackForce));

        // 变色反馈
        StartCoroutine(FlashColor());

        if (当前生命 <= 0)
        {
            Die();
        }
    }

    // 击退协程
    IEnumerator KnockbackRoutine(Vector2 dir, float chargePower, float baseKnockbackForce)
    {
        isKnockedBack = true;

        // 蓄力越久，击退力度越大（使用武器的基础击退力度）
        float finalForce = baseKnockbackForce * (0.5f + chargePower); // 0.5倍到1.5倍力度

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

    // 可视化攻击范围（在Scene视图中显示）
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, 攻击距离);
    }
}

