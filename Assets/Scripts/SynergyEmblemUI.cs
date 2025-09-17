using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class SynergyEmblemUI : MonoBehaviour
{
    public Image emblemImage;
    public Sprite activeSprite; // 激活状态的纹章图片
    public Sprite inactiveSprite; // 未激活状态的纹章图片

    void Start()
    {
        // 游戏开始时默认不激活
        Activate(false);
    }

    public void Activate(bool isActive)
    {
        emblemImage.sprite = isActive ? activeSprite : inactiveSprite;

        if (isActive)
        {
            // 激活时的动画效果：缩放弹跳 + 发光闪烁
            transform.DOScale(1.2f, 0.3f).SetEase(Ease.OutBack).OnComplete(() => {
                transform.DOScale(1.0f, 0.2f);
            });

            // 发光效果：快速闪烁几次
            emblemImage.DOColor(Color.yellow, 0.1f).SetLoops(6, LoopType.Yoyo).OnComplete(() => {
                emblemImage.color = Color.white;
            });
        }
        else
        {
            // 失活时的简单淡出效果
            emblemImage.DOColor(Color.gray, 0.3f);
            transform.DOScale(0.9f, 0.3f);
        }
    }
}