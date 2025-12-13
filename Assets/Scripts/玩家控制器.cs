using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class 玩家控制器 : MonoBehaviour
{
    [Header("武器设置")]
    public Transform 剑的旋转轴;
    public float 旋转速度 = 300f;
    public float 最大蓄力时间 = 2.0f;
    public float 最小冲刺力度 = 1f;
    public float 最大冲刺力度 = 10f;

    [Header("伤害设置")]
    public float 基础伤害 = 10f; // 基础伤害值
    public float 蓄力加成伤害 = 20f; // 蓄力满时的额外伤害加成

    private float 当前蓄力时间 = 0f;
    public float 上次蓄力百分比 { get; private set; }

    [Header("体力系统")]
    public Slider 体力条;
    public float 最大体力 = 100f;
    public float 体力消耗 = 10f;
    public float 体力恢复 = 15f;

    private float 当前体力;

    [Header("玩家生命")]
    public int 最大生命值 = 10;

    public int 当前生命值;

    [Header("状态 (只读)")]
    public bool isAiming = false; // 是否正在瞄准(停止旋转)
    public bool isDashing = false; // 是否正在冲刺
    public bool isInvincible = false; //是否为无敌状态

    private Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        当前体力 = 最大体力;
        当前生命值 = 最大生命值;
        UpdateUI();
    }

    void Update()
    {
        // 如果速度小于阈值，且之前是冲刺状态，则结束冲刺
        // 注意：这里只在速度降低时重置，避免覆盖PerformDash中设置的isDashing
        if (isDashing && rb.velocity.magnitude < 1.0f)
        {
            isDashing = false;
        }

        // 恢复体力逻辑：
        RegenerateStamina();

        // 如果正在冲刺中，暂时不处理输入或旋转
        if (isDashing) return;

        HandleInput();
        HandleRotation();
    }

    // 1. 处理输入 (支持鼠标/触摸)
    void HandleInput()
    {
        // 1. 按下：检查体力是否足够
        if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space))
        {
            if (当前体力 >= 体力消耗)
            {
                isAiming = true;
                当前蓄力时间 = 0f;
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
            剑的旋转轴.Rotate(Vector3.forward * 旋转速度 * Time.deltaTime);
        }
    }

    // 3. 执行冲刺逻辑
    void PerformDash()
    {
        // 扣除体力
        当前体力 -= 体力消耗;
        UpdateUI(); // 立即刷新UI

        // 计算蓄力百分比 (0 到 1)
        上次蓄力百分比 = 当前蓄力时间 / 最大蓄力时间;

        // 根据百分比计算实际力度 (Lerp 在最小和最大力度之间插值)
        float finalForce = Mathf.Lerp(最小冲刺力度, 最大冲刺力度, 上次蓄力百分比);

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
        if (当前体力 < 最大体力)
        {
            当前体力 += 体力恢复 * Time.deltaTime;
            // 确保不超过上限
            当前体力 = Mathf.Min(当前体力, 最大体力);
            UpdateUI();
        }
    }

    // 单独写一个方法更新UI，整洁一些
    void UpdateUI()
    {
        if (体力条 != null)
        {
            体力条.value = 当前体力 / 最大体力 * 100f; // 假设Slider是0-100

            // 也可以用 Slider 的 normalizedValue (0-1)
            // staminaSlider.maxValue = maxStamina;
            // staminaSlider.value = currentStamina;
        }
    }
    // 新增：玩家受伤方法
    public void TakeDamage(int damage)
    {
        if (isInvincible) return;
        
        当前生命值 -= damage;
        StartCoroutine(InvincibilityRoutine());
        Debug.Log($"玩家受伤！当前血量: {当前生命值}");
        if (当前生命值 <= 0)
        {
            Debug.Log("游戏结束！");
            // 这里可以重载场景或显示结算面板
            // UnityEngine.SceneManagement.SceneManager.LoadScene(0);
        }
    }
    // 协程：简单的冲刺状态管理
    IEnumerator DashRoutine()
    {
        // 这里可以加: 开启残影特效
        yield return new WaitForSeconds(0.2f); // 这里的等待仅用于视觉效果或短暂的硬直
        // 这里可以加: 关闭残影特效
        // 不要在这里设置 isDashing = false，交给 Update 里的速度去判断

    }

    IEnumerator InvincibilityRoutine()
    {
        isInvincible = true;
        // 这里可以让主角闪烁一下
        yield return new WaitForSeconds(1.0f); // 无敌1秒
        isInvincible = false;
    }

    // 计算伤害：根据蓄力百分比计算实际伤害
    public float CalculateDamage()
    {
        // 伤害 = 基础伤害 + (蓄力百分比 * 蓄力加成伤害)
        return 基础伤害 + (上次蓄力百分比 * 蓄力加成伤害);
    }
}