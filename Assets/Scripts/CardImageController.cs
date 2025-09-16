using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 卡片图片控制器 - 处理图片的自适应显示和比例约束
/// </summary>
public class CardImageController : MonoBehaviour
{
    public enum AspectMode
    {
        WidthPriority,    // 宽度优先 - 保持宽度，高度自适应
        HeightPriority,   // 高度优先 - 保持高度，宽度自适应  
        EnvelopeParent,   // 完全填充 - 保持比例，可能裁剪
        FitParent,        // 适应父级 - 保持比例，不裁剪
        None              // 无约束 - 可能拉伸变形
    }

    [Header("图片显示设置")]
    public AspectMode aspectMode = AspectMode.EnvelopeParent;
    
    [Header("最小尺寸限制")]
    public float minWidth = 50f;
    public float minHeight = 50f;
    
    [Header("最大尺寸限制")] 
    public float maxWidth = 500f;
    public float maxHeight = 500f;

    private Image targetImage;
    private RectTransform rectTransform;
    private AspectRatioFitter aspectFitter;
    private ContentSizeFitter sizeFitter;

    /// <summary>
    /// 获取当前图片的宽高比（只读）
    /// </summary>
    public float AspectRatio
    {
        get { return aspectFitter != null ? aspectFitter.aspectRatio : 1f; }
    }

    void Awake()
    {
        InitializeComponents();
    }

    void InitializeComponents()
    {
        targetImage = GetComponent<Image>();
        rectTransform = GetComponent<RectTransform>();
        
        // 确保有AspectRatioFitter组件
        aspectFitter = GetComponent<AspectRatioFitter>();
        if (aspectFitter == null)
        {
            aspectFitter = gameObject.AddComponent<AspectRatioFitter>();
        }

        // 添加ContentSizeFitter用于自适应尺寸
        sizeFitter = GetComponent<ContentSizeFitter>();
        if (sizeFitter == null)
        {
            sizeFitter = gameObject.AddComponent<ContentSizeFitter>();
        }

        SetupFitters();
    }

    void SetupFitters()
    {
        // 根据选择的模式设置AspectRatioFitter
        switch (aspectMode)
        {
            case AspectMode.WidthPriority:
                aspectFitter.aspectMode = AspectRatioFitter.AspectMode.WidthControlsHeight;
                sizeFitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
                sizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
                break;
                
            case AspectMode.HeightPriority:
                aspectFitter.aspectMode = AspectRatioFitter.AspectMode.HeightControlsWidth;
                sizeFitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
                sizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
                break;
                
            case AspectMode.EnvelopeParent:
                aspectFitter.aspectMode = AspectRatioFitter.AspectMode.EnvelopeParent;
                sizeFitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
                sizeFitter.verticalFit = ContentSizeFitter.FitMode.Unconstrained;
                break;
                
            case AspectMode.FitParent:
                aspectFitter.aspectMode = AspectRatioFitter.AspectMode.FitInParent;
                sizeFitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
                sizeFitter.verticalFit = ContentSizeFitter.FitMode.Unconstrained;
                break;
                
            case AspectMode.None:
                aspectFitter.aspectMode = AspectRatioFitter.AspectMode.None;
                sizeFitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
                sizeFitter.verticalFit = ContentSizeFitter.FitMode.Unconstrained;
                break;
        }
    }

    /// <summary>
    /// 设置图片并自动计算宽高比
    /// </summary>
    public void SetImage(Sprite sprite)
    {
        if (targetImage == null) InitializeComponents();
        
        targetImage.sprite = sprite;
        
        if (sprite != null && sprite.texture != null)
        {
            float aspectRatio = (float)sprite.texture.width / sprite.texture.height;
            aspectFitter.aspectRatio = aspectRatio;
            
            // 应用尺寸限制
            ApplySizeConstraints();
        }
        else
        {
            aspectFitter.aspectRatio = 1f;
        }
    }

    /// <summary>
    /// 设置显示模式
    /// </summary>
    public void SetAspectMode(AspectMode mode)
    {
        aspectMode = mode;
        SetupFitters();
        ApplySizeConstraints();
    }

    /// <summary>
    /// 应用尺寸限制
    /// </summary>
    public void ApplySizeConstraints()
    {
        if (rectTransform == null) return;

        // 获取当前尺寸
        Vector2 currentSize = rectTransform.sizeDelta;
        Vector2 originalSize = currentSize;
        
        // 应用最小尺寸限制
        if (currentSize.x < minWidth) currentSize.x = minWidth;
        if (currentSize.y < minHeight) currentSize.y = minHeight;
        
        // 应用最大尺寸限制
        if (currentSize.x > maxWidth) currentSize.x = maxWidth;
        if (currentSize.y > maxHeight) currentSize.y = maxHeight;
        
        // 只在尺寸确实改变时才设置，避免不必要的事件触发
        if (currentSize != originalSize)
        {
            rectTransform.sizeDelta = currentSize;
        }
    }

    /// <summary>
    /// 智能缩放图片 - 处理过小或过大的图片
    /// </summary>
    public void SmartScaleImage()
    {
        if (targetImage == null || targetImage.sprite == null) return;

        Texture2D texture = targetImage.sprite.texture;
        if (texture == null) return;

        // 获取图片原始尺寸
        Vector2 imageSize = new Vector2(texture.width, texture.height);
        
        // 获取容器当前尺寸（使用sizeDelta更可靠）
        Vector2 containerSize = rectTransform.sizeDelta;
        if (containerSize.x <= 0 || containerSize.y <= 0)
        {
            // 如果sizeDelta无效，使用父容器的尺寸
            RectTransform parentRect = transform.parent as RectTransform;
            if (parentRect != null)
            {
                containerSize = parentRect.rect.size;
            }
            else
            {
                containerSize = new Vector2(200f, 150f); // 默认尺寸
            }
        }
        
        // 计算缩放比例
        float scaleX = containerSize.x / imageSize.x;
        float scaleY = containerSize.y / imageSize.y;
        
        // 选择最小的缩放比例来保持比例
        float scale = Mathf.Min(scaleX, scaleY);
        
        // 如果图片太小，适当放大
        if (scale > 1.5f && imageSize.x < 100 && imageSize.y < 100)
        {
            scale = 1.5f; // 限制最大放大倍数
        }
        // 如果图片太大，适当缩小
        else if (scale < 0.3f && (imageSize.x > 2000 || imageSize.y > 2000))
        {
            scale = 0.3f; // 限制最小缩小倍数
        }
        
        // 应用缩放 - 使用sizeDelta而不是localScale，避免影响Transform层级
        if (scale != 1f)
        {
            Vector2 newSize = new Vector2(imageSize.x * scale, imageSize.y * scale);
            rectTransform.sizeDelta = newSize;
        }
    }

    /// <summary>
    /// 强制刷新布局
    /// </summary>
    public void RefreshLayout()
    {
        LayoutRebuilder.ForceRebuildLayoutImmediate(rectTransform);
        ApplySizeConstraints();
    }

    // 编辑器回调 - 移除所有运行时操作，避免SendMessage错误
    #if UNITY_EDITOR
    void OnValidate()
    {
        // 只设置fitters，不应用尺寸约束
        SetupFitters();
    }
    #endif
}