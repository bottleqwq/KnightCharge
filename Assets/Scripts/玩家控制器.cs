using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using TMPro;
using DamageNumbersPro;
using Cinemachine;

public class 玩家控制器 : MonoBehaviour
{
    [Header("武器设置")]
    public Transform 剑的旋转轴;
    
    // 这些值会在应用武器时被覆盖，但保留作为默认值
    [Header("默认武器属性（会被选择的武器覆盖）")]
    public float 旋转速度 = 300f;
    public float 最大蓄力时间 = 2.0f;
    public float 最小冲刺力度 = 10f;
    public float 最大冲刺力度 = 20f;
    public float 基础伤害 = 10f;
    public float 蓄力加成伤害 = 20f;
    public float 击退力度 = 10f;
    public float 体力消耗 = 10f;

    private float 当前蓄力时间 = 0f;
    public float 上次蓄力百分比 { get; private set; }

    [Header("体力系统")]
    public Slider 体力值条;
    public TextMeshProUGUI 体力值文本;

    public float 当前体力值;

    [Header("蓄力UI")]
    public Slider 蓄力条;

    [Header("玩家生命")]
    public Slider 生命值条;
    public TextMeshProUGUI 生命值文本;

    public float 当前生命值;

    [Header("护甲系统")]
    public Slider 护甲值条;
    public TextMeshProUGUI 护甲值文本;

    public float 当前护甲值;
    
    private float 上次受伤时间 = -999f;
    private float 护甲恢复累积值 = 0f; // 用于累积护甲恢复值，避免小数丢失

    [Header("护盾系统")]
    public GameObject 护盾物件;

    [Header("战斗手感")]
    public DamageNumber damageNumberPrefab;
    public ParticleSystem ps;
    public float 命中反冲力 = 3f;
    public float 顿帧时间 = 1f;
    public CinemachineVirtualCamera targetCamera;
    public float 镜头默认大小 = 8f;
    public float 镜头收缩大小 = 4f;
    public float 镜头收缩速度 = 2f;
    public float 镜头放大速度 = 10f;
    private CinemachineImpulseSource impulseSource;

    [Header("状态 (只读)")]
    public bool isAiming = false; // 是否正在瞄准(停止旋转)
    public bool isDashing = false; // 是否正在冲刺
    public bool isInvincible = false; //是否为无敌状态

    private Rigidbody2D rb;
    private SpriteRenderer 剑的SpriteRenderer; // 缓存剑的SpriteRenderer
    private Coroutine currentShieldCoroutine;
    void Start()
    {
        var emission = ps.emission;
        emission.enabled = false;
        rb = GetComponent<Rigidbody2D>();
        impulseSource = GetComponent<CinemachineImpulseSource>();
        当前体力值 = 玩家属性.Instance.最大体力值;
        当前生命值 = 玩家属性.Instance.最大生命值;
        当前护甲值 = 玩家属性.Instance.最大护甲值;
        上次受伤时间 = -999f; // 初始化为一个很早的时间，让护甲可以立即开始恢复
        护甲恢复累积值 = 0f; // 初始化护甲恢复累积值
        UpdateUI();
        蓄力条.value = 0;
        护盾物件.SetActive(false);

        // 应用选择的武器
        应用选择的武器();
    }

    /// <summary>
    /// 应用选择的武器：包括Sprite和所有属性
    /// </summary>
    void 应用选择的武器()
    {
        // 从管理器获取选择的武器数据
        武器数据 选择的武器数据 = 武器选择管理器.获取选择的武器();

        if (选择的武器数据 == null)
        {
            Debug.LogWarning("未选择武器，使用默认武器属性");
            return;
        }

        // 应用武器属性
        旋转速度 = 选择的武器数据.旋转速度;
        最大蓄力时间 = 选择的武器数据.最大蓄力时间;
        最小冲刺力度 = 选择的武器数据.最小冲刺力度;
        最大冲刺力度 = 选择的武器数据.最大冲刺力度;
        基础伤害 = 选择的武器数据.基础伤害;
        蓄力加成伤害 = 选择的武器数据.蓄力加成伤害;
        击退力度 = 选择的武器数据.击退力度;
        体力消耗 = 选择的武器数据.体力消耗;

        Debug.Log($"已应用武器属性: {选择的武器数据.武器名称} | " +
                  $"旋转速度: {旋转速度} | " +
                  $"基础伤害: {基础伤害} | " +
                  $"体力消耗: {体力消耗}");

        // 应用武器Sprite
        if (剑的旋转轴 != null)
        {
            // 在旋转轴的子物体中查找Tag为"Sword"的物体
            Transform 剑Transform = null;
            foreach (Transform child in 剑的旋转轴)
            {
                if (child.CompareTag("Sword"))
                {
                    剑Transform = child;
                    break;
                }
            }

            if (剑Transform != null)
            {
                剑的SpriteRenderer = 剑Transform.GetComponent<SpriteRenderer>();
                if (剑的SpriteRenderer != null)
                {
                    剑的SpriteRenderer.sprite = 选择的武器数据.武器Sprite;
                    Debug.Log($"已应用武器Sprite: {选择的武器数据.武器名称}");
                }
                else
                {
                    Debug.LogError("未找到剑的SpriteRenderer组件！");
                }

                // 应用碰撞器尺寸
                BoxCollider2D 剑的碰撞器 = 剑Transform.GetComponent<BoxCollider2D>();
                if (剑的碰撞器 != null)
                {
                    剑的碰撞器.size = 选择的武器数据.碰撞器尺寸;
                    Debug.Log($"已应用武器碰撞器尺寸: {选择的武器数据.武器名称} | Size: {选择的武器数据.碰撞器尺寸}");
                }
                else
                {
                    Debug.LogWarning("未找到剑的BoxCollider2D组件！");
                }
            }
            else
            {
                Debug.LogError("未找到Tag为'Sword'的子物体！");
            }
        }
        else
        {
            Debug.LogError("剑的旋转轴未设置！");
        }
    }

    void Update()
    {
        // 如果速度小于阈值，且之前是冲刺状态，则结束冲刺
        // 注意：这里只在速度降低时重置，避免覆盖PerformDash中设置的isDashing
        if (isDashing && rb.velocity.magnitude < 1.0f)
        {
            isDashing = false;
        }

        if (isAiming == false) { targetCamera.m_Lens.OrthographicSize = Mathf.Lerp(targetCamera.m_Lens.OrthographicSize, 镜头默认大小, Time.deltaTime * 镜头放大速度); }
        // 恢复体力逻辑：
        RegenerateStamina();

        // 恢复护甲值逻辑：
        RegenerateArmor();

        // 如果正在冲刺中，暂时不处理输入或旋转
        if (isDashing) return;

        HandleInput();
        HandleRotation();      
    }

    // 1. 处理输入 (支持鼠标/触摸)
    void HandleInput()
    {
        if (InputUtils.IsClickingUI()) return; // 如果在UI上，停止执行
        // 1. 按下：检查体力是否足够
        if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space))
        {
            if (当前体力值 >= 体力消耗)
            {
                isAiming = true;
                当前蓄力时间 = 0f;
                蓄力条.gameObject.SetActive(true);
                蓄力条.value = 0;
            }
            else
            {
                // 体力不足反馈：这里可以播放一个错误音效，或者让体力条闪烁红色
                Debug.Log("体力不足！");
                // 也可以做一个简单的UI抖动效果
            }
        }

        // 2. 按住：蓄力
        if (isAiming) // 只有成功进入瞄准状态才蓄力
        {
            if (Input.GetMouseButton(0) || Input.GetKey(KeyCode.Space))
            {
                当前蓄力时间 += Time.deltaTime;
                当前蓄力时间 = Mathf.Clamp(当前蓄力时间, 0, 最大蓄力时间);
                蓄力条.value = 当前蓄力时间 / 最大蓄力时间;
                targetCamera.m_Lens.OrthographicSize = Mathf.Lerp(targetCamera.m_Lens.OrthographicSize, 镜头收缩大小, Time.deltaTime * 镜头收缩速度);
            }
        }

        // 3. 松开：发射
        if (Input.GetMouseButtonUp(0) || Input.GetKeyUp(KeyCode.Space))
        {
            if (isAiming)
            {
                PerformDash();
                isAiming = false;                
            }
        }        
    }


    // 2. 处理剑的旋转
    void HandleRotation()
    {
        // 只有在不瞄准的时候才旋转
        if (!isAiming)
        {
            // 围绕 Z 轴旋转 WeaponPivot
            剑的旋转轴.Rotate(Vector3.forward * (旋转速度 + 玩家属性.Instance.旋转速度修正值) * Time.deltaTime);
        }
    }

    // 3. 执行冲刺逻辑
    void PerformDash()
    {
        // 扣除体力
        当前体力值 -= 体力消耗;
        UpdateUI(); // 立即刷新UI

        // 计算蓄力百分比 (0 到 1)
        上次蓄力百分比 = 当前蓄力时间 / 最大蓄力时间;

        // 根据百分比计算实际力度 (Lerp 在最小和最大力度之间插值)
        float finalForce = Mathf.Lerp(最小冲刺力度 + 玩家属性.Instance.冲刺力度增加值, 最大冲刺力度 + 玩家属性.Instance.冲刺力度增加值, 上次蓄力百分比);

        // 确定方向：剑目前相对于 Pivot 的右方向
        // 因为 Sword 是 WeaponPivot 的子物体，且 WeaponPivot 在旋转，
        // 所以 WeaponPivot.right 就是剑指向的方向 (前提是剑初始是水平向右放的)
        Vector2 dashDirection = 剑的旋转轴.right;

        // 立即设置冲刺状态，确保攻击检测能正常工作
        isDashing = true;

        // 给主角施加瞬间力
        rb.velocity = Vector2.zero; // 先清空当前速度，保证手感
        rb.AddForce(dashDirection * finalForce, ForceMode2D.Impulse);

        // 开启冲刺状态（可选：如果你想冲刺期间剑也不转，可以延时恢复）
        StartCoroutine(DashRoutine());
    }
    void RegenerateStamina()
    {
        // 如果体力没满，就增加
        if (当前体力值 < 玩家属性.Instance.最大体力值)
        {
            当前体力值 += 玩家属性.Instance.体力值恢复速度 * Time.deltaTime;
            // 确保不超过上限
            当前体力值 = Mathf.Min(当前体力值, 玩家属性.Instance.最大体力值);
            UpdateUI();
        }
    }

    /// <summary>
    /// 恢复护甲值：受伤后延迟一段时间开始恢复
    /// </summary>
    void RegenerateArmor()
    {
        // 检查是否已经过了恢复延迟时间
        if (Time.time - 上次受伤时间 >= 玩家属性.Instance.护甲恢复延迟)
        {
            // 如果护甲没满，就恢复
            if (当前护甲值 < 玩家属性.Instance.最大护甲值)
            {
                // 累积恢复值，避免小数丢失
                护甲恢复累积值 += 玩家属性.Instance.护甲恢复速度 * Time.deltaTime;
                
                // 当累积值达到1或以上时，恢复1点护甲
                if (护甲恢复累积值 >= 1f)
                {
                    int 恢复点数 = Mathf.FloorToInt(护甲恢复累积值);
                    当前护甲值 += 恢复点数;
                    护甲恢复累积值 -= 恢复点数; // 保留小数部分
                    
                    // 确保不超过上限
                    当前护甲值 = Mathf.Min(当前护甲值, 玩家属性.Instance.最大护甲值);
                    UpdateUI();
                }
            }
            else
            {
                // 如果护甲已满，重置累积值
                护甲恢复累积值 = 0f;
            }
        }
    }

    public void UpdateUI()
    {
        // 更新体力条
        if (体力值条 != null)
        {
            体力值条.value = 当前体力值 / 玩家属性.Instance.最大体力值 * 100f; // 假设Slider是0-100

            // 也可以用 Slider 的 normalizedValue (0-1)
            // staminaSlider.maxValue = maxStamina;
            // staminaSlider.value = currentStamina;
        }

        // 更新体力值文本
        if (体力值文本 != null)
        {
            体力值文本.text = $"体力: {(int)当前体力值}/{(int)玩家属性.Instance.最大体力值}";
        }

        // 更新生命值文本
        if (生命值文本 != null)
        {
            生命值文本.text = $"生命: {当前生命值}/{玩家属性.Instance.最大生命值}";
        }

        // 更新生命值条（如果使用Slider）
        if (生命值条 != null)
        {
            生命值条.maxValue = 玩家属性.Instance.最大生命值;
            生命值条.value = 当前生命值;
        }

        // 更新护甲值文本
        if (护甲值文本 != null)
        {
            护甲值文本.text = $"护甲: {当前护甲值}/{玩家属性.Instance.最大护甲值}";
        }

        // 更新护甲值条（如果使用Slider）
        if (护甲值条 != null)
        {
            护甲值条.maxValue = 玩家属性.Instance.最大护甲值;
            护甲值条.value = 当前护甲值;
        }
    }
    public void TakeDamage(float damage)
    {
        if (isInvincible) return;
        float 闪避 = Random.Range(0f, 100f);
        if (闪避 <= 玩家属性.Instance.闪避率)
        {
            damageNumberPrefab.Spawn(transform.position, "闪避!");
            return;
        }
        // 记录受伤时间，用于护甲恢复延迟
        上次受伤时间 = Time.time;
        护甲恢复累积值 = 0f; // 重置护甲恢复累积值，重新开始计算恢复延迟

        // 优先扣除护甲值，护甲值为0后再扣除生命值
        float 剩余伤害 = damage;
        
        if (当前护甲值 > 0)
        {
            // 先扣除护甲值
            float 护甲扣除 = Mathf.Min(当前护甲值, 剩余伤害);
            当前护甲值 -= 护甲扣除;
            剩余伤害 -= 护甲扣除;
            Debug.Log($"护甲受到伤害: {护甲扣除}, 剩余护甲: {当前护甲值}");
        }
        
        // 如果还有剩余伤害，扣除生命值
        if (剩余伤害 > 0)
        {
            当前生命值 -= 剩余伤害;
            当前生命值 = Mathf.Max(0, 当前生命值); // 确保生命值不会小于0
            Debug.Log($"生命值受到伤害: {剩余伤害}, 剩余生命: {当前生命值}");
        }
        
        UpdateUI(); // 更新UI显示
        StartCoroutine(InvincibilityRoutine());
        Debug.Log($"玩家受伤！总伤害: {damage}, 当前护甲: {当前护甲值}, 当前血量: {当前生命值}");
        
        if (当前生命值 <= 0)
        {
            Debug.Log("游戏结束！");
            // 这里可以重载场景或显示结算面板
            // UnityEngine.SceneManagement.SceneManager.LoadScene(0);
        }
    }
    // 计算伤害：根据蓄力百分比计算实际伤害
    public float CalculateDamage()
    {
        return 基础伤害 + 玩家属性.Instance.基础伤害增加值 + (上次蓄力百分比 * (蓄力加成伤害 + 玩家属性.Instance.蓄力伤害增加值));
    }
    /// <summary>
    /// 当剑击中敌人时调用此方法
    /// </summary>
    public void 触发攻击命中反馈()
    {
        // 1. 立即停止当前的冲锋动量
        rb.velocity = Vector2.zero;

        // 2. 结束冲刺状态（可选，视设计而定，建议结束以防止后续逻辑干扰）
        isDashing = false;

        // 3. 施加反冲力 (向剑指向的相反方向)
        // 剑的旋转轴.right 是剑的攻击方向，取反即为后退方向
        Vector2 recoilDir = -剑的旋转轴.right;
        rb.AddForce(recoilDir * 命中反冲力, ForceMode2D.Impulse);
        Debug.Log("攻击命中！执行刹车与反冲。");

        // 4. 打击感
        StartCoroutine(HitStop(顿帧时间*(0.5f+上次蓄力百分比)));
        impulseSource.GenerateImpulse(0.5f+上次蓄力百分比);
    }
    /// <summary>
    /// 恢复生命值（由血瓶调用）
    /// </summary>
    /// <param name="amount">恢复数量</param>
    public void 恢复生命(float amount)
    {
        // 死亡状态无法回血
        if (当前生命值 <= 0) return;

        当前生命值 += amount;

        // 确保不超过最大生命值
        if (玩家属性.Instance != null)
        {
            当前生命值 = Mathf.Min(当前生命值, 玩家属性.Instance.最大生命值);
        }

        Debug.Log($"玩家恢复生命: {amount}, 当前生命: {当前生命值}");
        UpdateUI(); // 立即刷新UI显示
    }

    /// <summary>
    /// 获得临时护盾（由护盾道具调用）
    /// </summary>
    /// <param name="duration">无敌持续时间</param>
    public void 获得临时护盾(float duration)
    {
        // 如果当前已经在护盾协程中，先停止它，重新计时
        if (currentShieldCoroutine != null)
        {
            StopCoroutine(currentShieldCoroutine);
        }

        // 开启新的护盾协程
        currentShieldCoroutine = StartCoroutine(ShieldInvincibilityRoutine(duration));
    }
    // 协程：简单的冲刺状态管理
    IEnumerator DashRoutine()
    {
        // TODO: 开启残影特效
        var emission = ps.emission;
        emission.enabled = true;
        音频管理器.Instance.播放冲刺音效();
        yield return new WaitForSeconds(0.2f); // 这里的等待仅用于视觉效果或短暂的硬直
        // TODO: 关闭残影特效
        emission.enabled = false;
        // 不要在这里设置 isDashing = false，交给 Update 里的速度去判断

    }

    IEnumerator InvincibilityRoutine()
    {
        isInvincible = true;
        // TODO:这里可以让主角闪烁一下
        yield return new WaitForSeconds(1.0f); // 无敌1秒
        // 只有当没有护盾协程在运行时，才取消无敌状态
        // 避免受伤无敌结束时意外取消了还在持续的护盾无敌
        if (currentShieldCoroutine == null)
        {
            isInvincible = false;
        }
    }
    // 护盾道具的长时间无敌
    IEnumerator ShieldInvincibilityRoutine(float duration)
    {
        isInvincible = true;

        护盾物件.SetActive(true);

        yield return new WaitForSeconds(duration);

        // 护盾时间结束，取消无敌
        isInvincible = false;
        currentShieldCoroutine = null; // 清空引用

        护盾物件.SetActive(false);
    }
    // 打击感顿帧
    public IEnumerator HitStop(float duration)
    {
        if (Time.timeScale == 0) yield break; // 防止冲突

        float originalScale = Time.timeScale;
        Time.timeScale = 0.1f;

        // 使用由于TimeScale为0，不能用WaitForSeconds，要用realtime
        yield return new WaitForSecondsRealtime(duration);
        Time.timeScale = originalScale;
    }
}