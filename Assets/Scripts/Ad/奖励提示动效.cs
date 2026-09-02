using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace KnightCharge.Ad
{
    /// <summary>
    /// 奖励提示 UI 浮动与渐隐动效组件
    /// 挂载在「获得奖励提示UI」节点上，实现向上平滑漂浮、弹性弹出与渐隐淡出效果
    /// </summary>
    [RequireComponent(typeof(CanvasGroup))]
    public class 奖励提示动效 : MonoBehaviour
    {
        [Header("动效参数配置")]
        [Tooltip("向上漂浮的距离（像素）")]
        public float 上浮距离 = 70f;

        [Tooltip("动效总时长（秒）")]
        public float 动画时长 = 1.2f;

        [Tooltip("淡入时长比例（占总时长的比例，0~1）")]
        [Range(0.05f, 0.4f)]
        public float 淡入比例 = 0.15f;

        [Tooltip("淡出开始时间比例（占总时长的比例，0~1）")]
        [Range(0.4f, 0.9f)]
        public float 淡出起始比例 = 0.5f;

        [Header("视觉与曲线")]
        [Tooltip("是否启用弹出缩放缩放效果（0.8 -> 1.15 -> 1.0）")]
        public bool 启用弹出缩放 = true;

        [Tooltip("文本组件（可选，若挂载在文本上或子节点包含文本可自动绑定）")]
        public TextMeshProUGUI 提示文本;

        private CanvasGroup _canvasGroup;
        private RectTransform _rectTransform;
        private Vector2 _初始位置;
        private Vector3 _初始缩放;
        private Coroutine _当前动效协程;

        private void Awake()
        {
            _canvasGroup = GetComponent<CanvasGroup>();
            if (_canvasGroup == null)
            {
                _canvasGroup = gameObject.AddComponent<CanvasGroup>();
            }

            _rectTransform = GetComponent<RectTransform>();
            if (_rectTransform != null)
            {
                _初始位置 = _rectTransform.anchoredPosition;
            }

            _初始缩放 = transform.localScale == Vector3.zero ? Vector3.one : transform.localScale;

            if (提示文本 == null)
            {
                提示文本 = GetComponentInChildren<TextMeshProUGUI>();
            }
        }

        private void OnEnable()
        {
            // 防止默认激活时直接常驻显示
            if (_rectTransform != null && _初始位置 == Vector2.zero)
            {
                _初始位置 = _rectTransform.anchoredPosition;
            }
        }

        /// <summary>
        /// 播放奖励提示动画
        /// </summary>
        /// <param name="customText">可选的自定义提示文字（如 "+10 金币！"）</param>
        public void 播放提示(string customText = null)
        {
            if (!string.IsNullOrEmpty(customText) && 提示文本 != null)
            {
                提示文本.text = customText;
            }

            gameObject.SetActive(true);

            if (_当前动效协程 != null)
            {
                StopCoroutine(_当前动效协程);
            }

            _当前动效协程 = StartCoroutine(DoFloatingAnimation());
        }

        private IEnumerator DoFloatingAnimation()
        {
            if (_rectTransform == null)
            {
                _rectTransform = GetComponent<RectTransform>();
            }

            if (_canvasGroup == null)
            {
                _canvasGroup = GetComponent<CanvasGroup>();
            }

            // 1. 初始化状态
            Vector2 startPos = _初始位置;
            Vector2 targetPos = startPos + new Vector2(0, 上浮距离);

            _rectTransform.anchoredPosition = startPos;
            _canvasGroup.alpha = 0f;
            transform.localScale = 启用弹出缩放 ? _初始缩放 * 0.7f : _初始缩放;

            float elapsed = 0f;

            while (elapsed < 动画时长)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / 动画时长);

                // --- 1. 位置上移（缓动曲线：越往上越慢，平滑停驻） ---
                float moveProgress = Mathf.Sin(t * Mathf.PI * 0.5f); // EaseOutSine
                _rectTransform.anchoredPosition = Vector2.Lerp(startPos, targetPos, moveProgress);

                // --- 2. 透明度变化（快速淡入 -> 保持 -> 逐渐透明消失） ---
                if (t <= 淡入比例)
                {
                    // 阶段一：0 -> 1 淡入
                    float inT = t / 淡入比例;
                    _canvasGroup.alpha = Mathf.SmoothStep(0f, 1f, inT);
                }
                else if (t >= 淡出起始比例)
                {
                    // 阶段二：1 -> 0 逐渐透明消失
                    float outT = (t - 淡出起始比例) / (1f - 淡出起始比例);
                    _canvasGroup.alpha = Mathf.SmoothStep(1f, 0f, outT);
                }
                else
                {
                    // 中间停留阶段
                    _canvasGroup.alpha = 1f;
                }

                // --- 3. 弹出缩放效果（0.7 -> 1.15 -> 1.0） ---
                if (启用弹出缩放)
                {
                    if (t <= 淡入比例)
                    {
                        float popT = t / 淡入比例;
                        // 弹性放大到 1.15 倍
                        transform.localScale = Vector3.Lerp(_初始缩放 * 0.7f, _初始缩放 * 1.15f, popT);
                    }
                    else if (t <= 淡入比例 + 0.15f)
                    {
                        float settleT = (t - 淡入比例) / 0.15f;
                        // 回落到正常 1.0 倍
                        transform.localScale = Vector3.Lerp(_初始缩放 * 1.15f, _初始缩放, settleT);
                    }
                    else
                    {
                        transform.localScale = _初始缩放;
                    }
                }

                yield return null;
            }

            // 动画完成，重置并隐藏
            _canvasGroup.alpha = 0f;
            _rectTransform.anchoredPosition = startPos;
            transform.localScale = _初始缩放;
            gameObject.SetActive(false);
            _当前动效协程 = null;
        }

        private void OnDisable()
        {
            if (_当前动效协程 != null)
            {
                StopCoroutine(_当前动效协程);
                _当前动效协程 = null;
            }

            if (_rectTransform != null)
            {
                _rectTransform.anchoredPosition = _初始位置;
            }

            if (_canvasGroup != null)
            {
                _canvasGroup.alpha = 0f;
            }
        }
    }
}
