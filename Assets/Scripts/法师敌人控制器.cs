using System.Collections;
using DamageNumbersPro;
using UnityEngine;

public class 法师敌人控制器 : MonoBehaviour
{
    private enum EnemyState
    {
        Chasing,    // 追逐
        Casting,    // 施法中 (原地不动)
        Cooldown    // 施法后冷却
    }

    [Header("敌人属性")]
    public float 移动速度 = 1.5f; // 法师通常走得慢一点
    public float 最大生命 = 40f;
    public int 接触伤害 = 1; // 碰到敌人身体的伤害

    [Header("法术设置")]
    public GameObject 法阵预制体; // 需要在Inspector拖入一个圆形的Sprite物体
    public float 施法距离 = 6f;   // 距离玩家多远开始放技能
    public float 法阵半径 = 1.5f; // 伤害判定的范围
    public float 施法前摇 = 1.0f; // 法阵出现后多久爆炸
    public int 法术伤害 = 2;      // 法阵爆炸造成的伤害
    public float 施法冷却 = 3.0f; // 放完一次技能休息多久

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

    // 状态与施法相关
    private EnemyState currentState = EnemyState.Chasing;
    private float cooldownTimer = 0f;
    private GameObject 当前生成的法阵; // 记录当前生成的法阵实例

    [Header("死亡特效")]
    public float 死亡淡出时间 = 0.75f;
    public Color 死亡燃烧颜色 = Color.cyan; // 法师死的时候发蓝光
    private Material 死亡材质实例;

    [Header("分数设置")]
    public int 击杀分数 = 8;

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

        // 如果处于冷却状态，倒计时
        if (currentState == EnemyState.Cooldown)
        {
            cooldownTimer -= Time.deltaTime;
            if (cooldownTimer <= 0)
            {
                currentState = EnemyState.Chasing;
            }
        }

        // 如果被控制（击退或僵直），暂停行为
        if (isKnockedBack || isStunned) return;

        // 状态机
        switch (currentState)
        {
            case EnemyState.Chasing:
                HandleChasing();
                break;
            case EnemyState.Casting:
                // 施法中什么都不做，等待协程完成
                break;
            case EnemyState.Cooldown:
                HandleCooldownMovement(); // 冷却时可以选择发呆或者远离玩家
                break;
        }
    }

    void HandleChasing()
    {
        if (playerTransform == null) return;

        float distance = Vector2.Distance(transform.position, playerTransform.position);

        // 如果距离够近，并且不在冷却中，开始施法
        if (distance <= 施法距离)
        {
            StartCoroutine(CastSpellRoutine());
        }
        else
        {
            // 否则向玩家移动
            transform.position = Vector2.MoveTowards(transform.position, playerTransform.position, 移动速度 * Time.deltaTime);
        }
    }

    void HandleCooldownMovement()
    {
        // 可选：冷却期间稍微远离玩家，保持风筝距离
        // 或者简单的原地不动：
        // Do nothing
    }

    IEnumerator CastSpellRoutine()
    {
        currentState = EnemyState.Casting;
        rb.velocity = Vector2.zero; // 施法时停下

        Vector2 targetPos = playerTransform.position;

        // 1. 生成法阵
        if (法阵预制体 != null)
        {
            当前生成的法阵 = Instantiate(法阵预制体, targetPos, Quaternion.identity);

            // --- 新增代码：获取法阵脚本并设置时间 ---
            法阵行为 magicScript = 当前生成的法阵.GetComponent<法阵行为>();
            if (magicScript != null)
            {
                // 把 施法前摇 和 法阵半径 传过去
                magicScript.初始化(施法前摇, 法阵半径);
            }
            else
            {
                // 如果没挂脚本，就用老方法的简单缩放作为保底
                当前生成的法阵.transform.localScale = Vector3.one * (法阵半径 * 2);
            }
            // ------------------------------------
        }

        // 2. 等待前摇 (这段时间内，法阵脚本在自动演示缩圈动画)
        yield return new WaitForSeconds(施法前摇);

        // 3. 判定伤害
        if (!isDead && 当前生成的法阵 != null)
        {
            CheckSpellDamage(targetPos);
            Destroy(当前生成的法阵);
        }

        currentState = EnemyState.Cooldown;
        cooldownTimer = 施法冷却;
    }

    void CheckSpellDamage(Vector2 center)
    {
        // 播放爆炸音效
        // 音频管理器.Instance.播放爆炸音效(); 

        // 检测圆圈内的所有物体
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(center, 法阵半径);
        foreach (var hitCollider in hitColliders)
        {
            if (hitCollider.CompareTag("Player"))
            {
                玩家控制器 playerScript = hitCollider.GetComponent<玩家控制器>();
                if (playerScript != null)
                {
                    playerScript.TakeDamage(法术伤害);
                    Debug.Log("玩家踩到法阵了！");

                    // 可选：给玩家一点推力
                    Vector2 dir = (playerScript.transform.position - transform.position).normalized;
                    Rigidbody2D prb = playerScript.GetComponent<Rigidbody2D>();
                    if (prb) prb.AddForce(dir * 5f, ForceMode2D.Impulse);
                }
            }
        }
    }

    // --- 辅助功能：在编辑器里画出范围 ---
    void OnDrawGizmosSelected()
    {
        // 画出施法距离（黄色）
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, 施法距离);

        // 画出伤害半径示意（红色）
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, 法阵半径);
    }

    // --- 必须保留的受击与死亡逻辑 (保持一致性) ---

    // 碰撞伤害 (身体撞击)
    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            玩家控制器 playerScript = collision.gameObject.GetComponent<玩家控制器>();
            if (playerScript != null)
            {
                playerScript.TakeDamage(接触伤害); // 身体触碰伤害较低
                StartCoroutine(StunRoutine(攻击僵直时间));
                Vector2 pushBack = (transform.position - collision.transform.position).normalized;
                rb.AddForce(pushBack * 3f, ForceMode2D.Impulse);
            }
        }
    }

    void OnTriggerEnter2D(Collider2D other) { if (other.CompareTag("Sword")) 处理剑的碰撞(other); }
    void OnTriggerStay2D(Collider2D other) { if (other.CompareTag("Sword")) 处理剑的碰撞(other); }

    void 处理剑的碰撞(Collider2D swordCollider)
    {
        玩家控制器 playerScript = swordCollider.GetComponentInParent<玩家控制器>();
        if (playerScript != null && playerScript.isDashing)
        {
            if (Time.time - 上次受到伤害时间 >= 伤害冷却时间)
            {
                float chargePercent = playerScript.上次蓄力百分比;
                float damage = playerScript.CalculateDamage();
                Vector2 knockbackDir = playerScript.剑的旋转轴.right;
                float knockbackForce = playerScript.击退力度 + 玩家属性.Instance.击退增加值;

                TakeDamage(damage, knockbackDir, chargePercent, knockbackForce);

                // 体力恢复与吸血逻辑...
                if (playerScript.当前体力值 < 玩家属性.Instance.最大体力值)
                {
                    playerScript.当前体力值 = Mathf.Min(playerScript.当前体力值 + 玩家属性.Instance.命中恢复体力, 玩家属性.Instance.最大体力值);
                    playerScript.UpdateUI();
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

        // 关键点：如果受到伤害，打断施法！
        // 这样玩家可以通过攻击来取消敌人的法阵，增加互动性
        if (currentState == EnemyState.Casting)
        {
            StopAllCoroutines(); // 停止施法协程
            if (当前生成的法阵 != null) Destroy(当前生成的法阵); // 删除预警圈
            currentState = EnemyState.Chasing; // 回到追逐状态
        }

        StartCoroutine(StunRoutine(受击僵直时间));
        StartCoroutine(KnockbackRoutine(knockbackDir, chargePercent, knockbackForce));
        StartCoroutine(FlashColor());

        if (当前生命 <= 0) Die();
    }

    IEnumerator FlashColor()
    {
        spriteRenderer.color = Color.red;
        yield return new WaitForSeconds(受击变色时间);
        spriteRenderer.color = Color.white;
    }

    IEnumerator KnockbackRoutine(Vector2 dir, float chargePower, float baseKnockbackForce)
    {
        isKnockedBack = true;
        float finalForce = baseKnockbackForce * (0.5f + chargePower);
        rb.velocity = Vector2.zero;
        rb.AddForce(dir * finalForce, ForceMode2D.Impulse);
        yield return new WaitForSeconds(0.3f);
        isKnockedBack = false;
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
        if (当前生成的法阵 != null) Destroy(当前生成的法阵); // 死的时候把没炸的圈删掉

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

        CheckDrop(金币, 玩家属性.Instance.金币爆率);
        CheckDrop(血瓶, 玩家属性.Instance.血瓶爆率);
        CheckDrop(护盾, 玩家属性.Instance.护盾爆率);

        Destroy(gameObject);
    }

    void CheckDrop(GameObject prefab, float rate)
    {
        if (prefab != null && Random.Range(0f, 100f) <= rate)
            Instantiate(prefab, transform.position, Quaternion.identity);
    }
}
