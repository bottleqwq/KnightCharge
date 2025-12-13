using UnityEngine;

/// <summary>
/// 武器选择管理器：用于在场景之间传递选择的武器信息
/// </summary>
public static class 武器选择管理器
{
    // 保存选择的武器数据
    private static 武器数据 选择的武器数据 = null;

    /// <summary>
    /// 设置选择的武器
    /// </summary>
    /// <param name="weaponData">武器数据</param>
    public static void 设置选择的武器(武器数据 weaponData)
    {
        选择的武器数据 = weaponData;
        if (weaponData != null)
        {
            Debug.Log($"已选择武器: {weaponData.武器名称}");
        }
    }

    /// <summary>
    /// 获取选择的武器数据
    /// </summary>
    /// <returns>选择的武器数据，如果未选择则返回null</returns>
    public static 武器数据 获取选择的武器()
    {
        return 选择的武器数据;
    }

    /// <summary>
    /// 获取选择的武器名称
    /// </summary>
    /// <returns>武器名称</returns>
    public static string 获取选择的武器名称()
    {
        return 选择的武器数据?.武器名称 ?? "";
    }

    /// <summary>
    /// 清除选择的武器（可选，用于重置）
    /// </summary>
    public static void 清除选择()
    {
        选择的武器数据 = null;
    }
}