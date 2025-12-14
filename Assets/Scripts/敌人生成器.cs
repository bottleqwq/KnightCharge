using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// 敌人生成器：动态生成敌人，随着时间推移生成频率增加，敌人属性增强
/// </summary>
public class 敌人生成器 : MonoBehaviour
{
    [Header("敌人预制体设置")]
    [Tooltip("近战敌人预制体列表（按难度从低到高）")]
    public GameObject[] 近战敌人预制体列表;
    
    [Tooltip("远程敌人预制体列表（按难度从低到高）")]
    public GameObject[] 远程敌人预制体列表;

    [Header("生成设置")]
    [Tooltip("初始生成间隔（秒）")]
    public float 初始生成间隔 = 5f;
    
    [Tooltip("最小生成间隔（秒），不会低于这个值")]
    public float 最小生成间隔 = 1f;
    
    [Tooltip("生成间隔减少速度（每秒减少多少秒）")]
    public float 生成间隔减少速度 = 0.1f;
    
    [Tooltip("初始最大敌人数量")]
    public int 初始最大敌人数量 = 3;
    
    [Tooltip("最终最大敌人数量")]
    public int 最终最大敌人数量 = 20;
    
    [Tooltip("生成位置距离玩家的最小距离")]
    public float 生成距离最小值 = 10f;
    
    [Tooltip("生成位置距离玩家的最大距离")]
    public float 生成距离最大值 = 15f;

    [Header("难度增长设置")]
    [Tooltip("每多少秒增加一个难度等级")]
    public float 难度增长间隔 = 30f;
    
    [Tooltip("生命值增长倍数（每级）")]
    public float 生命值增长倍数 = 1.2f;
    
    [Tooltip("伤害增长倍数（每级）")]
    public float 伤害增长倍数 = 1.15f;
    
    [Tooltip("速度增长倍数（每级）")]
    public float 速度增长倍数 = 1.1f;
    
    [Tooltip("远程敌人攻击间隔减少倍数（每级，值越小攻击越快）")]
    public float 攻击间隔减少倍数 = 0.9f;

    [Header("生成位置设置")]
    [Tooltip("是否在屏幕边缘生成")]
    public bool 在屏幕边缘生成 = true;
    
    [Tooltip("如果不在屏幕边缘，使用随机圆形生成")]
    public bool 使用随机圆形生成 = false;

    [Header("敌人类型权重")]
    [Tooltip("近战敌人生成概率（0-1）")]
    [Range(0f, 1f)]
    public float 近战敌人概率 = 0.7f;
    [Tooltip("远程敌人生成所需的最低难度等级（达到此等级后才会生成远程敌人）")]
    public int 远程敌人生成所需难度等级 = 2;

    private Transform playerTransform;
    private Camera mainCamera;
    private float 当前生成间隔;
    private float 上次生成时间 = 0f;
    private float 游戏开始时间;
    private int 当前难度等级 = 0;
    private int 当前最大敌人数量;
    private List<GameObject> 当前敌人列表 = new List<GameObject>();

    void Start()
    {
        // 初始化
        当前生成间隔 = 初始生成间隔;
        当前最大敌人数量 = 初始最大敌人数量;
        游戏开始时间 = Time.time;
        上次生成时间 = Time.time;

        // 查找玩家
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            playerTransform = playerObj.transform;
        }
        else
        {
            Debug.LogWarning("敌人生成器：未找到玩家对象！");
        }

        // 获取主摄像机
        mainCamera = Camera.main;
        if (mainCamera == null)
        {
            mainCamera = FindObjectOfType<Camera>();
        }

        // 开始生成协程
        StartCoroutine(生成敌人协程());
    }

    void Update()
    {
        // 更新生成间隔（随时间减少）
        float 经过时间 = Time.time - 游戏开始时间;
        当前生成间隔 = Mathf.Max(最小生成间隔, 初始生成间隔 - (经过时间 * 生成间隔减少速度));

        // 更新难度等级
        int 新难度等级 = Mathf.FloorToInt(经过时间 / 难度增长间隔);
        if (新难度等级 != 当前难度等级)
        {
            当前难度等级 = 新难度等级;
            Debug.Log($"难度等级提升至: {当前难度等级}");
        }

        // 更新最大敌人数量（随难度等级增长）
        // 每提升一个难度等级，最大敌人数量增加一定数量
        int 额外敌人数量 = Mathf.RoundToInt((最终最大敌人数量 - 初始最大敌人数量) * (当前难度等级 / 10f));
        当前最大敌人数量 = Mathf.Min(初始最大敌人数量 + 额外敌人数量, 最终最大敌人数量);

        // 清理已死亡的敌人
        清理死亡敌人();
    }

    /// <summary>
    /// 生成敌人的协程
    /// </summary>
    IEnumerator 生成敌人协程()
    {
        while (true)
        {
            // 检查是否可以生成（敌人数量未达上限）
            if (当前敌人列表.Count < 当前最大敌人数量)
            {
                生成敌人();
            }

            // 等待生成间隔
            yield return new WaitForSeconds(当前生成间隔);
        }
    }

    /// <summary>
    /// 生成一个敌人
    /// </summary>
    void 生成敌人()
    {
        if (playerTransform == null)
        {
            Debug.LogWarning("敌人生成器：玩家对象不存在，无法生成敌人！");
            return;
        }

        // 选择敌人类型
        GameObject 敌人预制体 = 选择敌人预制体();
        if (敌人预制体 == null)
        {
            Debug.LogWarning("敌人生成器：没有可用的敌人预制体！");
            return;
        }

        // 计算生成位置
        Vector2 生成位置 = 计算生成位置();
        
        // 实例化敌人
        GameObject 新敌人 = Instantiate(敌人预制体, 生成位置, Quaternion.identity);
        
        // 应用难度增强
        增强敌人属性(新敌人);
        
        // 添加到列表
        当前敌人列表.Add(新敌人);
        
        Debug.Log($"生成敌人: {敌人预制体.name} 在位置 {生成位置}, 难度等级: {当前难度等级}");
    }

    /// <summary>
    /// 选择要生成的敌人预制体
    /// </summary>
    GameObject 选择敌人预制体()
    {
        // 检查是否可以生成远程敌人（需要达到指定难度等级）
        bool 可以生成远程 = 当前难度等级 >= 远程敌人生成所需难度等级;
        
        // 决定生成近战还是远程
        bool 生成近战;
        if (!可以生成远程)
        {
            // 如果还不能生成远程敌人，强制生成近战
            生成近战 = true;
        }
        else
        {
            // 可以生成远程时，按概率选择
            生成近战 = Random.value < 近战敌人概率;
        }
        
        GameObject[] 可用列表 = 生成近战 ? 近战敌人预制体列表 : 远程敌人预制体列表;
        
        if (可用列表 == null || 可用列表.Length == 0)
        {
            // 如果选择的类型没有预制体，尝试另一种类型
            if (可以生成远程)
            {
                可用列表 = 生成近战 ? 远程敌人预制体列表 : 近战敌人预制体列表;
            }
            else
            {
                // 如果还不能生成远程，只能尝试近战
                可用列表 = 近战敌人预制体列表;
            }
        }
        
        if (可用列表 == null || 可用列表.Length == 0)
        {
            return null;
        }

        // 根据难度等级选择敌人类型（难度越高，选择更强的敌人）
        int 选择索引 = Mathf.Min(当前难度等级 / 2, 可用列表.Length - 1);
        
        // 添加一些随机性（可能生成当前等级或低一级的敌人）
        if (选择索引 > 0 && Random.value < 0.3f)
        {
            选择索引--;
        }

        return 可用列表[选择索引];
    }

    /// <summary>
    /// 计算敌人生成位置
    /// </summary>
    Vector2 计算生成位置()
    {
        Vector2 玩家位置 = playerTransform.position;
        Vector2 生成位置;

        if (在屏幕边缘生成 && mainCamera != null)
        {
            // 在屏幕边缘生成
            生成位置 = 获取屏幕边缘随机位置();
        }
        else if (使用随机圆形生成)
        {
            // 随机圆形生成
            float 角度 = Random.Range(0f, 360f) * Mathf.Deg2Rad;
            float 距离 = Random.Range(生成距离最小值, 生成距离最大值);
            生成位置 = 玩家位置 + new Vector2(Mathf.Cos(角度), Mathf.Sin(角度)) * 距离;
        }
        else
        {
            // 默认：随机圆形生成
            float 角度 = Random.Range(0f, 360f) * Mathf.Deg2Rad;
            float 距离 = Random.Range(生成距离最小值, 生成距离最大值);
            生成位置 = 玩家位置 + new Vector2(Mathf.Cos(角度), Mathf.Sin(角度)) * 距离;
        }

        return 生成位置;
    }

    /// <summary>
    /// 获取屏幕边缘的随机位置
    /// </summary>
    Vector2 获取屏幕边缘随机位置()
    {
        if (mainCamera == null) return playerTransform.position;

        // 获取屏幕边界
        float 屏幕高度 = mainCamera.orthographicSize * 2f;
        float 屏幕宽度 = 屏幕高度 * mainCamera.aspect;
        
        Vector2 玩家屏幕位置 = mainCamera.WorldToScreenPoint(playerTransform.position);
        Vector2 玩家世界位置 = playerTransform.position;

        // 随机选择屏幕的哪一边（上、下、左、右）
        int 边 = Random.Range(0, 4);
        Vector2 边缘位置;

        switch (边)
        {
            case 0: // 上边
                边缘位置 = new Vector2(
                    Random.Range(-屏幕宽度 / 2f, 屏幕宽度 / 2f),
                    屏幕高度 / 2f + 生成距离最小值
                );
                break;
            case 1: // 下边
                边缘位置 = new Vector2(
                    Random.Range(-屏幕宽度 / 2f, 屏幕宽度 / 2f),
                    -屏幕高度 / 2f - 生成距离最小值
                );
                break;
            case 2: // 左边
                边缘位置 = new Vector2(
                    -屏幕宽度 / 2f - 生成距离最小值,
                    Random.Range(-屏幕高度 / 2f, 屏幕高度 / 2f)
                );
                break;
            default: // 右边
                边缘位置 = new Vector2(
                    屏幕宽度 / 2f + 生成距离最小值,
                    Random.Range(-屏幕高度 / 2f, 屏幕高度 / 2f)
                );
                break;
        }

        return 玩家世界位置 + 边缘位置;
    }

    /// <summary>
    /// 增强敌人属性（根据难度等级）
    /// </summary>
    void 增强敌人属性(GameObject 敌人)
    {
        if (当前难度等级 <= 0) return; // 难度等级0不增强

        float 生命值倍数 = Mathf.Pow(生命值增长倍数, 当前难度等级);
        float 伤害倍数 = Mathf.Pow(伤害增长倍数, 当前难度等级);
        float 速度倍数 = Mathf.Pow(速度增长倍数, 当前难度等级);

        // 增强近战敌人
        敌人控制器 近战控制器 = 敌人.GetComponent<敌人控制器>();
        if (近战控制器 != null)
        {
            近战控制器.最大生命 *= 生命值倍数;
            近战控制器.伤害 = Mathf.RoundToInt(近战控制器.伤害 * 伤害倍数);
            近战控制器.移动速度 *= 速度倍数;
        }

        // 增强远程敌人
        远程敌人控制器 远程控制器 = 敌人.GetComponent<远程敌人控制器>();
        if (远程控制器 != null)
        {
            远程控制器.最大生命 *= 生命值倍数;
            远程控制器.伤害 = Mathf.RoundToInt(远程控制器.伤害 * 伤害倍数);
            远程控制器.移动速度 *= 速度倍数;
            
            // 减少攻击间隔（攻击更快）
            float 间隔倍数 = Mathf.Pow(攻击间隔减少倍数, 当前难度等级);
            远程控制器.发射间隔 *= 间隔倍数;
        }
    }

    /// <summary>
    /// 清理已死亡的敌人
    /// </summary>
    void 清理死亡敌人()
    {
        for (int i = 当前敌人列表.Count - 1; i >= 0; i--)
        {
            if (当前敌人列表[i] == null)
            {
                当前敌人列表.RemoveAt(i);
            }
        }
    }

    /// <summary>
    /// 获取当前难度等级（供其他脚本使用）
    /// </summary>
    public int 获取当前难度等级()
    {
        return 当前难度等级;
    }

    /// <summary>
    /// 获取当前敌人数量（供其他脚本使用）
    /// </summary>
    public int 获取当前敌人数量()
    {
        return 当前敌人列表.Count;
    }

    /// <summary>
    /// 可视化生成范围（在Scene视图中显示）
    /// </summary>
    void OnDrawGizmosSelected()
    {
        if (playerTransform == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                playerTransform = playerObj.transform;
            }
        }

        if (playerTransform != null)
        {
            // 绘制生成范围
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(playerTransform.position, 生成距离最小值);
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(playerTransform.position, 生成距离最大值);
        }
    }
}
