using UnityEngine;
using UnityEngine.UI;

public class InstructionPanelController : MonoBehaviour
{
    [Header("UI 元素")]
    [SerializeField] private GameObject[] pages; // 存放所有页面的数组
    [SerializeField] private Button prevButton;  // “上一页”按钮
    [SerializeField] private Button nextButton;  // “下一页”按钮

    private int currentPageIndex = 0; // 当前页面的索引
    
    private void Start()
    {
        // 动态绑定按钮点击事件
        if (prevButton != null) prevButton.onClick.AddListener(PrevPage);
        if (nextButton != null) nextButton.onClick.AddListener(NextPage);
    }
    
    private void OnEnable()
    {
        // 每次打开说明界面时，默认重置显示第一页
        ShowPage(0);
    }

    /// <summary>
    /// 显示指定索引的页面，并更新按钮状态
    /// </summary>
    public void ShowPage(int index)
    {
        // 安全检查
        if (pages == null || pages.Length == 0) return;
        if (index < 0 || index >= pages.Length) return;

        // 隐藏所有页面，只激活当前索引的页面
        for (int i = 0; i < pages.Length; i++)
        {
            if (pages[i] != null)
            {
                pages[i].SetActive(i == index);
            }
        }

        currentPageIndex = index;

        // 更新“上一页”和“下一页”按钮的显示状态
        UpdateButtonVisibility();
    }

    /// <summary>
    /// 根据当前页码决定按钮的显示与隐藏
    /// </summary>
    private void UpdateButtonVisibility()
    {
        if (pages == null || pages.Length == 0) return;

        // 如果总共只有一页
        if (pages.Length == 1)
        {
            if (prevButton != null) prevButton.gameObject.SetActive(false);
            if (nextButton != null) nextButton.gameObject.SetActive(false);
            return;
        }

        // 情况1：第一页，隐藏“上一页”，显示“下一页”
        if (currentPageIndex == 0)
        {
            if (prevButton != null) prevButton.gameObject.SetActive(false);
            if (nextButton != null) nextButton.gameObject.SetActive(true);
        }
        // 情况2：最后一页，显示“上一页”，隐藏“下一页”
        else if (currentPageIndex == pages.Length - 1)
        {
            if (prevButton != null) prevButton.gameObject.SetActive(true);
            if (nextButton != null) nextButton.gameObject.SetActive(false);
        }
        // 情况3：中间页，两边按钮都显示
        else
        {
            if (prevButton != null) prevButton.gameObject.SetActive(true);
            if (nextButton != null) nextButton.gameObject.SetActive(true);
        }
    }

    /// <summary>
    /// 下一页按钮点击事件
    /// </summary>
    private void NextPage()
    {
        if (currentPageIndex < pages.Length - 1)
        {
            ShowPage(currentPageIndex + 1);
        }
    }

    /// <summary>
    /// 上一页按钮点击事件
    /// </summary>
    private void PrevPage()
    {
        if (currentPageIndex > 0)
        {
            ShowPage(currentPageIndex - 1);
        }
    }
    
}