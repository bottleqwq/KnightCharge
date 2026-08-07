using System.Collections;
using DamageNumbersPro;
using UnityEngine;

public class 冲刺敌人控制器 : MonoBehaviour
{
    // 定义敌人的状态枚举
    private enum EnemyState
    {
        Chasing,    // 追逐
        Preparing,  // 蓄力准备
        Dashing,    // 正在冲刺
        Cooldown    // 攻击后冷却
    }

    [Header("敌人属性")]
    public float 普通移动速度 = 2f;
    public float 最大生命 = 50f;
    public int 伤害 = 1;

    [Header("冲刺技能设置")]
    public float 冲刺触发距离 = 5f;     // 玩家进入这个范围开始蓄力
    public float 蓄力时间 = 0.5f;       // 锁定目标前等待的时间
    public float 冲刺速度 = 12f;        // 冲刺时的速度
    public float 冲刺后冷却时间 = 1.5f; // 冲刺完休息多久
    public Color 蓄力预警颜色 = Color.magenta; // 蓄力时变什么颜色

    [Header("受击反馈")]
    public float 受击变色时间 = 0.1f;
    public float 受击僵直时间 = 0.5f;
    public float 攻击僵直时间 = 0.3f;
    public DamageNumber damageNumberPrefab;

    private Transform playerTransform;
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private float 当前生命;
    private bool isDead = false;
    private bool isKnockedBack = false;
    private bool isStunned = false;
    private float 上次受到伤害时间 = -1f;
    private const float 伤害冷却时间 = 0.5f;

    // 状态机相关变量
    private EnemyState currentState = EnemyState.Chasing;
    private Vector2 lockedTargetPosition; // 锁定的冲刺目标点
    private float stateTimer; // 通用计时器

    [Header("死亡特效 (All In 1 Sprite Shader)")]
    public float 死亡淡出时间 = 0.75f;
    public Color 死亡燃烧颜色 = Color.yellow;
    private Material 死亡材质实例;

    [Header("分数设置")]
    public int 击杀分数 = 7;

    [Header("掉落设置")]
    public GameObject 金币;
    public GameObject 血瓶;
    public GameObject 护盾;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        当前生命 = 最大生命;

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            playerTransform = playerObj.transform;
        }
    }

    void Update()
    {
        if (isDead) return;

        // 如果处于受击击退或僵直状态，暂停状态机逻辑，但保持计时器暂停或重置视需求而定
        // 这里简单处理：只要被控，就不能执行主动行为
        if (isKnockedBack || isStunned)
        {
            // 如果在蓄力时被打断，是被打断还是继续？通常打断会比较好，恢复后重新追逐
            if (currentState == EnemyState.Preparing)
            {
                RecoverToChasing();
            }
            return;
        }

        if (playerTransform == null) return;

        // 状态机逻辑
        switch (currentState)
        {
            case EnemyState.Chasing:
                HandleChasing();
                break;
            case EnemyState.Preparing:
                HandlePreparing();
                break;
            case EnemyState.Dashing:
                HandleDashing();
                break;
            case EnemyState.Cooldown:
                HandleCooldown();
                break;
        }
    }

    // --- 状态逻辑 ---

    void HandleChasing()
    {
        float distance = Vector2.Distance(transform.position, playerTransform.position);

        // 向玩家移动
        transform.position = Vector2.MoveTowards(transform.position, playerTransform.position, 普通移动速度 * Time.deltaTime);

        // 如果距离小于触发距离，切换到蓄力状态
        if (distance <= 冲刺触发距离)
        {
            currentState = EnemyState.Preparing;
            stateTimer = 蓄力时间;
            spriteRenderer.color = 蓄力预警颜色; // 视觉提示
        }
    }

    void HandlePreparing()
    {
        // 蓄力期间停止移动，或者你可以让它极其缓慢地移动
        // 倒计时
        stateTimer -= Time.deltaTime;

        if (stateTimer <= 0)
        {
            // 蓄力结束，锁定当前玩家位置
            lockedTargetPosition = playerTransform.position;

            // 为了防止冲刺距离过短（比如玩家贴脸），可以把目标点向远延伸一点
            Vector2 direction = (lockedTargetPosition - (Vector2)transform.position).normalized;
            // 可选：让它冲得比玩家位置稍微远一点点，保证穿过玩家
            lockedTargetPosition += direction * 2f;

            StartCoroutine(FlashColor(Color.white, 0f)); // 恢复颜色
            currentState = EnemyState.Dashing;
        }
    }

    void HandleDashing()
    {
        // 快速向锁定点移动
        transform.position = Vector2.MoveTowards(transform.position, lockedTargetPosition, 冲刺速度 * Time.deltaTime);

        // 检查是否到达目标点 (距离非常近)
        if (Vector2.Distance(transform.position, lockedTargetPosition) < 0.1f)
        {
            currentState = EnemyState.Cooldown;
            stateTimer = 冲刺后冷却时间;
        }
    }

    void HandleCooldown()
    {
        // 冷却期间发呆，或者可以慢速徘徊
        stateTimer -= Time.deltaTime;
        if (stateTimer <= 0)
        {
            RecoverToChasing();
        }
    }

    void RecoverToChasing()
    {
        currentState = EnemyState.Chasing;
        spriteRenderer.color = Color.white; // 确保颜色恢复
    }

    // --- 以下是原有的碰撞和伤害逻辑 (基本保持不变) ---

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            玩家控制器 playerScript = collision.gameObject.GetComponent<玩家控制器>();
            if (playerScript != null)
            {
                playerScript.TakeDamage(伤害);
                // 撞到玩家后，如果是冲刺状态，应该停止冲刺进入冷却，或者继续冲过去？
                // 这里设定为撞到人就停下进入僵直
                StartCoroutine(StunRoutine(攻击僵直时间));

                Vector2 pushBack = (transform.position - collision.transform.position).normalized;
                rb.AddForce(pushBack * 5f, ForceMode2D.Impulse);

                // 撞到人后重置为追逐或冷却
                RecoverToChasing();
            }
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Sword")) 处理剑的碰撞(other);
    }

    void OnTriggerStay2D(Collider2D other)
    {
        if (other.CompareTag("Sword")) 处理剑的碰撞(other);
    }

    void 处理剑的碰撞(Collider2D swordCollider)
    {
        玩家控制器 playerScript = swordCollider.GetComponentInParent<玩家控制器>();
        if (playerScript != null && playerScript.isDashing)
        {
            if (Time.time - 上次受到伤害时间 >= 伤害冷却时间)
            {
                // 如果在蓄力或冲刺时被打中，这里会被击退打断
                float chargePercent = playerScript.上次蓄力百分比;
                float damage = playerScript.CalculateDamage();
                Vector2 knockbackDir = playerScript.剑的旋转轴.right;
                float knockbackForce = playerScript.击退力度 + 玩家属性.Instance.击退增加值;

                TakeDamage(damage, knockbackDir, chargePercent, knockbackForce);

                // ... (回血等逻辑保持不变)
                if (playerScript.当前体力值 < 玩家属性.Instance.最大体力值)
                {
                    playerScript.当前体力值 = Mathf.Min(playerScript.当前体力值 + 玩家属性.Instance.命中恢复体力, 玩家属性.Instance.最大体力值);
                    playerScript.UpdateUI();
                }
                float 吸血随机数 = Random.Range(0f, 100f);
                if (吸血随机数 <= 玩家属性.Instance.生命窃取率)
                {
                    damageNumberPrefab.Spawn(transform.position, "吸血!");
                    playerScript.恢复生命(玩家属性.Instance.生命窃取值);
                }
                playerScript.触发攻击命中反馈();

                上次受到伤害时间 = Time.time;
            }
        }
    }

    void TakeDamage(float damage, Vector2 knockbackDir, float chargePercent, float knockbackForce)
    {
        if (isDead) return;
        当前生命 -= damage;
        damageNumberPrefab.Spawn(transform.position, damage);
        音频管理器.Instance.播放命中音效();

        // 受击强制打断当前的冲刺或蓄力
        currentState = EnemyState.Chasing;
        spriteRenderer.color = Color.white;

        StartCoroutine(StunRoutine(受击僵直时间));
        StartCoroutine(KnockbackRoutine(knockbackDir, chargePercent, knockbackForce));
        StartCoroutine(FlashColor(Color.red, 受击变色时间));

        if (当前生命 <= 0) Die();
    }

    // 重载一下FlashColor以支持自定义颜色和时间
    IEnumerator FlashColor(Color color, float time)
    {
        spriteRenderer.color = color;
        yield return new WaitForSeconds(time);
        // 如果还活着且不在蓄力状态，恢复白色
        if (!isDead && currentState != EnemyState.Preparing)
        {
            spriteRenderer.color = Color.white;
        }
    }

    // 为了兼容旧代码的简单调用
    IEnumerator FlashColor()
    {
        yield return FlashColor(Color.red, 受击变色时间);
    }

    IEnumerator KnockbackRoutine(Vector2 dir, float chargePower, float baseKnockbackForce)
    {
        isKnockedBack = true;
        float finalForce = baseKnockbackForce * (0.5f + chargePower);
        rb.velocity = Vector2.zero;
        rb.AddForce(dir * finalForce, ForceMode2D.Impulse);
        yield return new WaitForSeconds(0.3f);
        isKnockedBack = false;
        // 击退结束后，速度归零，防止滑行
        rb.velocity = Vector2.zero;
    }

    IEnumerator StunRoutine(float stunDuration)
    {
        isStunned = true;
        if (rb != null) rb.velocity = Vector2.zero;
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

        if (分数管理器.Instance != null)
        {
            分数管理器.Instance.增加分数(击杀分数);
        }
    }

    IEnumerator DeathEffectRoutine()
    {
        Collider2D col = GetComponent<Collider2D>();
        if (col) col.enabled = false;
        if (rb) { rb.velocity = Vector2.zero; rb.isKinematic = true; }

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
        while (timer < 死亡淡出时间)
        {
            timer += Time.deltaTime;
            if (死亡材质实例) 死亡材质实例.SetFloat("_FadeAmount", Mathf.Lerp(-0.1f, 1f, timer / 死亡淡出时间));
            yield return null;
        }

        // 掉落逻辑保持不变
        CheckDrop(金币, 玩家属性.Instance.金币爆率);
        CheckDrop(血瓶, 玩家属性.Instance.血瓶爆率);
        CheckDrop(护盾, 玩家属性.Instance.护盾爆率);

        Destroy(gameObject);
    }

    void CheckDrop(GameObject prefab, float rate)
    {
        if (prefab != null && Random.Range(0f, 100f) <= rate)
        {
            Instantiate(prefab, transform.position, Quaternion.identity);
            音频管理器.Instance.播放金币掉落音效();
        }
    }
}
