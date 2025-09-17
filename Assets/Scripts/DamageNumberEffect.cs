using UnityEngine;
using TMPro; // 导入 TextMeshPro 命名空间
using DG.Tweening; // 导入 DOTween 命名空间

public class DamageNumberEffect : MonoBehaviour
{
    // 在 Inspector（检视面板）里，把 Prefab 上的 TextMeshPro 组件拖到这里
    public TextMeshPro textMesh;

    // --- 可调整的动画参数 ---
    public float floatDuration = 1.0f;  // 动画总时长
    public float floatHeight = 1.5f;   // 漂浮的总高度
    public float fadeDelay = 0.5f;   // 延迟多久后开始淡出 (0.5表示动画过半后才开始)

    // 这个方法将由"生成者"调用
    public void ShowDamage(int damageAmount)
    {
        // 1. 设置伤害文本
        if (textMesh == null)
        {
            textMesh = GetComponent<TextMeshPro>(); // 自动获取以防万一
        }
        textMesh.text = damageAmount.ToString();

        // 确保开始时是完全不透明的
        textMesh.alpha = 1.0f;

#if !UNITY_WEBGL || UNITY_EDITOR
        // --- 使用 DOTween 序列(Sequence)来编排动画 ---
        Sequence mySequence = DOTween.Sequence();

        // 动画1: 向上移动 (DOMoveY)
        // SetRelative(true) 表示在当前位置的基础上"增加"floatHeight
        // SetEase(Ease.OutQuad) 是一种"快出慢入"的缓动，看起来更自然
        Tweener moveTween = transform.DOMoveY(transform.position.y + floatHeight, floatDuration)
                                    .SetRelative(true)
                                    .SetEase(Ease.OutQuad);

        // 动画2: 渐变消失 (DOFade)
        // 在 floatDuration 的时间内，将 alpha 渐变为 0
        // SetDelay(fadeDelay) 让我们在移动开始一段时间后再开始淡出
        Tweener fadeTween = textMesh.DOFade(0, floatDuration - fadeDelay)
                                  .SetDelay(fadeDelay)
                                  .SetEase(Ease.InQuad); // InQuad "慢出快入"，适合消失

        // 1. 将两个动画添加到序列中
        // Append 是"追加"，Join 是"同时插入"。这里我们希望移动和淡出同时发生（但淡出有延迟）
        // 所以我们把移动（moveTween）加进去，然后"Join"淡出（fadeTween）
        mySequence.Append(moveTween);
        mySequence.Join(fadeTween);

        // 2. (关键) 动画播放完毕后，销毁这个物体
        // OnComplete 是一个回调函数
        mySequence.OnComplete(() => {
            Destroy(gameObject);
        });

        // 3. （可选）你甚至可以让它在"弹出"时有一个缩放效果
        // transform.localScale = Vector3.zero;
        // mySequence.Join(transform.DOScale(Vector3.one, 0.3f).SetEase(Ease.OutBack)); // 像气泡一样弹出
#else
        // WebGL平台使用简单的协程动画
        StartCoroutine(SimpleFloatAnimation());
#endif
    }

#if UNITY_WEBGL && !UNITY_EDITOR
    // WebGL平台的简单动画
    private System.Collections.IEnumerator SimpleFloatAnimation()
    {
        Vector3 startPos = transform.position;
        Vector3 endPos = startPos + new Vector3(0, floatHeight, 0);
        float startAlpha = textMesh.alpha;

        float elapsedTime = 0;

        while (elapsedTime < floatDuration)
        {
            float progress = elapsedTime / floatDuration;

            // 位置插值
            transform.position = Vector3.Lerp(startPos, endPos, progress);

            // 透明度插值（延迟后开始）
            if (elapsedTime > fadeDelay)
            {
                float fadeProgress = (elapsedTime - fadeDelay) / (floatDuration - fadeDelay);
                textMesh.alpha = Mathf.Lerp(startAlpha, 0, fadeProgress);
            }

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        Destroy(gameObject);
    }
#endif
}