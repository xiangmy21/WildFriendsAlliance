using UnityEngine;
using UnityEngine.EventSystems;

public class DraggableUnitIcon : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [HideInInspector]
    public UnitData unitData; // 这个图标代表的单位数据

    private Transform originalParent; // 记录拖拽前的父物体 (它所在的槽位)
    private CanvasGroup canvasGroup;
    private Transform rootCanvas; // 整个UI的根Canvas

    void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        rootCanvas = GetComponentInParent<Canvas>().transform;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        originalParent = transform.parent;

        // 1. 提到最顶层渲染，防止被其他UI遮挡
        transform.SetParent(rootCanvas);
        // 2. 忽略射线检测，这样 OnDrop 才能检测到它下方的 "BenchSlot"
        canvasGroup.blocksRaycasts = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        // 3. 图标跟随鼠标
        transform.position = eventData.position;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        // 4. 拖拽结束，恢复射线检测
        canvasGroup.blocksRaycasts = true;

        // 5. eventData.pointerEnter 会告诉我们鼠标最后悬停在哪个物体上
        if (eventData.pointerEnter == null)
        {
            // 扔到了UI之外，代表“上阵”
            DeployToBattlefield(eventData.position);
            return;
        }

        // 检查是否扔到了出售区域 (假设你有一个 "SellZone" 标签的UI)
        if (eventData.pointerEnter.CompareTag("SellZone"))
        {
            GameManager.Instance.AddGold(1); // 卖1块钱
            Destroy(gameObject); // 销毁图标
            return;
        }

        // 检查是否扔到了另一个备战席 (BenchSlot)
        // OnDrop 已经处理了归位，这里不需要额外代码

        // 如果扔到的地方无效 (比如扔在两个槽位之间)
        if (transform.parent == rootCanvas)
        {
            // 放回原位
            transform.SetParent(originalParent);
            transform.localPosition = Vector3.zero;
        }
    }

    // (新增) 你需要给这个脚本添加 Initialize 方法，让 BenchManager 调用
    public int starLevel = 1; // 假设默认为1星
    public void Initialize(UnitData data)
    {
        this.unitData = data;
        // this.starLevel = data.starLevel; // 如果你的 UnitData 里有星级
        // GetComponent<Image>().sprite = data.icon; // 更新图标
    }

    // (修正) 这是 纯2D 版本 的放置逻辑
    private void DeployToBattlefield(Vector2 screenPosition)
    {
        // 1. 将 2D 屏幕坐标 转换为 2D 世界坐标
        Vector2 worldPosition = Camera.main.ScreenToWorldPoint(screenPosition);

        // 2. 检查该位置是否在 "可放置区域" (DroppableArea) 内
        //    我们用 OverlapPoint (检测一个点) 是否在 "DroppableArea" 图层的 Collider 内
        if (!Physics2D.OverlapPoint(worldPosition, LayerMask.GetMask("DroppableArea")))
        {
            Debug.Log("放置失败：区域无效！");
            ReturnToBench(); // 放回原位
            return;
        }

        // 3. 检查该位置是否会“重叠” (Unit)
        //    检测半径 0.5f (或你的单位半径)，确保不会跟 "Unit" 图层的单位重叠
        if (Physics2D.OverlapCircle(worldPosition, 0.5f, LayerMask.GetMask("Unit")))
        {
            Debug.Log("放置失败：位置重叠！");
            ReturnToBench(); // 放回原位
            return;
        }

        // 4. 所有检查通过：放置单位！
        //    (注意：Instantiate 时使用 worldPosition，它已经是 Vector2/Vector3)
        Instantiate(unitData.unitPrefab, worldPosition, Quaternion.identity);

        // 5. 销毁这个UI图标
        Destroy(gameObject);
    }

    // 一个辅助方法，用于放回原位
    public void ReturnToBench()
    {
        transform.SetParent(originalParent);
        transform.localPosition = Vector3.zero;
    }
}