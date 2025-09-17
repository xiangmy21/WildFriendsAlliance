using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 卡片图片演示控制器 - 展示CardImageController的所有功能
/// </summary>
public class CardImageDemo : MonoBehaviour
{
    [Header("演示设置")]
    public CardImageController cardImageController;
    public TMP_Dropdown aspectModeDropdown;
    public Button[] testImageButtons;
    public Slider minWidthSlider;
    public Slider minHeightSlider;
    public Slider maxWidthSlider;
    public Slider maxHeightSlider;
    public TMP_Text infoText;

    [Header("测试图片")]
    public Sprite[] testSprites;

    void Start()
    {
        InitializeUI();
        UpdateInfoText();
    }

    void InitializeUI()
    {
        // 初始化下拉菜单
        if (aspectModeDropdown != null)
        {
            aspectModeDropdown.ClearOptions();
            aspectModeDropdown.AddOptions(new System.Collections.Generic.List<string>
            {
                "宽度优先",
                "高度优先", 
                "完全填充",
                "适应父级",
                "无约束"
            });
            aspectModeDropdown.onValueChanged.AddListener(OnAspectModeChanged);
        }

        // 初始化测试图片按钮
        if (testImageButtons != null && testImageButtons.Length > 0)
        {
            for (int i = 0; i < testImageButtons.Length; i++)
            {
                int index = i;
                testImageButtons[i].onClick.AddListener(() => OnTestImageClicked(index));
            }
        }

        // 初始化尺寸滑块
        if (minWidthSlider != null)
        {
            minWidthSlider.onValueChanged.AddListener(OnMinWidthChanged);
            minWidthSlider.value = cardImageController.minWidth;
        }

        if (minHeightSlider != null)
        {
            minHeightSlider.onValueChanged.AddListener(OnMinHeightChanged);
            minHeightSlider.value = cardImageController.minHeight;
        }

        if (maxWidthSlider != null)
        {
            maxWidthSlider.onValueChanged.AddListener(OnMaxWidthChanged);
            maxWidthSlider.value = cardImageController.maxWidth;
        }

        if (maxHeightSlider != null)
        {
            maxHeightSlider.onValueChanged.AddListener(OnMaxHeightChanged);
            maxHeightSlider.value = cardImageController.maxHeight;
        }
    }

    void OnAspectModeChanged(int index)
    {
        if (cardImageController != null)
        {
            cardImageController.SetAspectMode((CardImageController.AspectMode)index);
            UpdateInfoText();
        }
    }

    void OnTestImageClicked(int index)
    {
        if (cardImageController != null && testSprites != null && index < testSprites.Length)
        {
            cardImageController.SetImage(testSprites[index]);
            cardImageController.SmartScaleImage();
            UpdateInfoText();
        }
    }

    void OnMinWidthChanged(float value)
    {
        if (cardImageController != null)
        {
            cardImageController.minWidth = value;
            cardImageController.RefreshLayout();
            UpdateInfoText();
        }
    }

    void OnMinHeightChanged(float value)
    {
        if (cardImageController != null)
        {
            cardImageController.minHeight = value;
            cardImageController.RefreshLayout();
            UpdateInfoText();
        }
    }

    void OnMaxWidthChanged(float value)
    {
        if (cardImageController != null)
        {
            cardImageController.maxWidth = value;
            cardImageController.RefreshLayout();
            UpdateInfoText();
        }
    }

    void OnMaxHeightChanged(float value)
    {
        if (cardImageController != null)
        {
            cardImageController.maxHeight = value;
            cardImageController.RefreshLayout();
            UpdateInfoText();
        }
    }

    void UpdateInfoText()
    {
        if (infoText != null && cardImageController != null)
        {
            string modeName = "";
            switch (cardImageController.aspectMode)
            {
                case CardImageController.AspectMode.WidthPriority:
                    modeName = "宽度优先";
                    break;
                case CardImageController.AspectMode.HeightPriority:
                    modeName = "高度优先";
                    break;
                case CardImageController.AspectMode.EnvelopeParent:
                    modeName = "完全填充";
                    break;
                case CardImageController.AspectMode.FitParent:
                    modeName = "适应父级";
                    break;
                case CardImageController.AspectMode.None:
                    modeName = "无约束";
                    break;
            }

            infoText.text = $"当前模式: {modeName}\n" +
                           $"宽高比: {cardImageController.AspectRatio:F2}\n" +
                           $"最小尺寸: {cardImageController.minWidth:F0}x{cardImageController.minHeight:F0}\n" +
                           $"最大尺寸: {cardImageController.maxWidth:F0}x{cardImageController.maxHeight:F0}";
        }
    }

    // 公共方法供UI调用
    public void ApplySmartScale()
    {
        if (cardImageController != null)
        {
            cardImageController.SmartScaleImage();
            UpdateInfoText();
        }
    }

    public void ResetToDefaults()
    {
        if (cardImageController != null)
        {
            cardImageController.minWidth = 50f;
            cardImageController.minHeight = 50f;
            cardImageController.maxWidth = 500f;
            cardImageController.maxHeight = 500f;
            cardImageController.SetAspectMode(CardImageController.AspectMode.EnvelopeParent);

            if (minWidthSlider != null) minWidthSlider.value = 50f;
            if (minHeightSlider != null) minHeightSlider.value = 50f;
            if (maxWidthSlider != null) maxWidthSlider.value = 500f;
            if (maxHeightSlider != null) maxHeightSlider.value = 500f;
            if (aspectModeDropdown != null) aspectModeDropdown.value = 2;

            UpdateInfoText();
        }
    }

    #if UNITY_EDITOR
    void OnValidate()
    {
        if (cardImageController == null)
        {
            cardImageController = GetComponent<CardImageController>();
        }
    }
    #endif
}