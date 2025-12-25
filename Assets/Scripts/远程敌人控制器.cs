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
    private bool isDead = false;
    private bool isKnockedBack = false; // 是否处于被击退状态
    private bool isStunned = false; // 是否处于僵直状态
    private bool isInAttackRange = false; // 是否在攻击范围内
    private float 上次发射时间 = 0f; // 上次发射弹幕的时间
    private float 上次受到伤害时间 = -1f; // 上次受到伤害的时间戳，用于防止重复伤害
    private const float 伤害冷却时间 = 0.5f; // 同一冲刺中的伤害冷却时间（秒）
    
    [Header("死亡特效 (All In 1 Sprite Shader)")]
    public float 死亡淡出时间 = 0.75f;
    public Color 死亡燃烧颜色 = Color.yellow;
    private Material 死亡材质实例;

    [Header("掉落设置")]
    public GameObject 金币;
    public GameObject 血瓶;
    public GameObject 护盾;

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
        if (isDead) return;
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
            处理剑的碰撞(other);
        }
    }

    // 持续碰撞检测：当剑的Collider已经与敌人重合时，检测玩家是否开始冲刺
    void OnTriggerStay2D(Collider2D other)
    {
        if (other.CompareTag("Sword"))
        {
            处理剑的碰撞(other);
        }
    }

    // 统一的碰撞处理逻辑
    void 处理剑的碰撞(Collider2D swordCollider)
    {
        玩家控制器 playerScript = swordCollider.GetComponentInParent<玩家控制器>();

        if (playerScript != null)
        {
            if (playerScript.isDashing)
            {
                // 检查伤害冷却时间，防止在同一冲刺中重复造成伤害
                if (Time.time - 上次受到伤害时间 >= 伤害冷却时间)
                {
                    float chargePercent = playerScript.上次蓄力百分比;
                    // 从玩家控制器获取伤害值
                    float damage = playerScript.CalculateDamage();
                    Vector2 knockbackDir = playerScript.剑的旋转轴.right;
                    float knockbackForce = playerScript.击退力度; // 从玩家控制器获取击退力度

                    // 执行伤害
                    TakeDamage(damage, knockbackDir, chargePercent, knockbackForce);
                    上次受到伤害时间 = Time.time; // 更新上次受到伤害的时间
                }
            }
        }
    }

    void TakeDamage(float damage, Vector2 knockbackDir, float chargePercent, float knockbackForce)
    {
        if (isDead) return;
        当前生命 -= damage;
        Debug.Log($"远程敌人受到伤害: {damage}, 剩余血量: {当前生命}");
        音频管理器.Instance.播放命中音效();

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
        if (isDead) return;
        isDead = true;
        StopAllCoroutines();
        StartCoroutine(DeathEffectRoutine());
        祝福管理器.Instance.增加祝福值(玩家属性.Instance.祝福击杀增长);
    }
    IEnumerator DeathEffectRoutine()
    {
        // 禁用碰撞与移动，避免死亡后继续交互
        Collider2D col = GetComponent<Collider2D>();
        if (col) col.enabled = false;
        if (rb)
        {
            rb.velocity = Vector2.zero;
            rb.isKinematic = true;
        }

        // 为当前实例克隆一份材质，避免影响其他敌人
        if (spriteRenderer && spriteRenderer.material)
        {
            if (死亡材质实例 == null)
            {
                死亡材质实例 = new Material(spriteRenderer.material);
                死亡材质实例.SetColor("_FadeBurnColor", 死亡燃烧颜色);
            }
            spriteRenderer.material = 死亡材质实例;
        }

        float timer = 0f;
        // All In 1 Sprite Shader 的 Fade Amount：-0.1 为完全显示，1 为完全烧蚀
        float 初始淡出 = -0.1f;
        float 目标淡出 = 1f;
        while (timer < 死亡淡出时间)
        {
            timer += Time.deltaTime;
            float t = Mathf.Clamp01(timer / 死亡淡出时间);
            float fadeValue = Mathf.Lerp(初始淡出, 目标淡出, t);
            if (死亡材质实例) 死亡材质实例.SetFloat("_FadeAmount", fadeValue);
            yield return null;
        }

        float 随机数 = Random.Range(0f, 100f);
        // 如果随机数小于等于掉落概率，则生成金币
        if (随机数 <= 玩家属性.Instance.金币爆率)
        {
            掉落金币();
            音频管理器.Instance.播放金币掉落音效();
        }
        float 血瓶随机数 = Random.Range(0f, 100f);
        if (血瓶随机数 <= 玩家属性.Instance.血瓶爆率)
        {
            掉落血瓶();
            音频管理器.Instance.播放金币掉落音效();
        }
        float 护盾随机数 = Random.Range(0f, 100f);
        if (护盾随机数 <= 玩家属性.Instance.护盾爆率)
        {
            掉落护盾();
            音频管理器.Instance.播放金币掉落音效();
        }
        Destroy(gameObject);
    }
    void 掉落金币()
    {
        if (金币 != null)
        {
            // 在敌人当前位置生成金币
            // Quaternion.identity 表示不旋转（或者你可以设置自定义旋转）
            Instantiate(金币, transform.position, Quaternion.identity);
        }
    }
    void 掉落血瓶()
    {
        if (血瓶 != null)
        {
            Instantiate(血瓶, transform.position, Quaternion.identity);
        }
    }
    void 掉落护盾()
    {
        if (护盾 != null)
        {
            Instantiate(护盾, transform.position, Quaternion.identity);
        }
    }

    // 可视化攻击范围（在Scene视图中显示）
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, 攻击距离);
    }
}

