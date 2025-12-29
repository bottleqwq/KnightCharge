using UnityEngine;
using UnityEngine.EventSystems;

public class InputUtils : MonoBehaviour
{
    // 判断是否点击在了UI上（兼容PC和移动端）
    public static bool IsClickingUI()
    {
        // 1. 检查PC端鼠标
        if (EventSystem.current.IsPointerOverGameObject())
            return true;

        // 2. 检查移动端触摸
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            if (touch.phase == TouchPhase.Began || touch.phase == TouchPhase.Moved)
            {
                // 传入手指ID进行检测
                if (EventSystem.current.IsPointerOverGameObject(touch.fingerId))
                    return true;
            }
        }

        return false;
    }
}